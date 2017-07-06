using knesset_app.DBEntities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace knesset_app
{
    /**
     * A class to read XML protocols and create objects for them so we can save them to the DB.
     */
    public class ProtocolFileParser
    {
        protected string fileName; // the path to the xml file to load
        protected ParagraphReader paragraphReader = new ParagraphReader(); // helps split paragraph into words

        private Dictionary<string, int> speakerParahraphs = new Dictionary<string, int>(); // count the paragraphs for each speaker, we need this info for the paragraph object

        // all existing objects  so we don't try to add them again and so we can reference them:
        private Dictionary<string, Person> existingPersons;
        private Dictionary<string, Word> existingWords;

        // all new objects so we can save them and reference them:
        public List<Person> newPersons = new List<Person>();
        public List<Word> newWords = new List<Word>();
        public List<ParagraphWord> newParagraphWords = new List<ParagraphWord>();
        public List<Paragraph> newParagraphs = new List<Paragraph>();
        public List<Presence> newPresence = new List<Presence>();
        public List<Invitation> newInvitations = new List<Invitation>();

        private static XNamespace // namespaces we need to use when parsing the xml
            pkg = "http://schemas.microsoft.com/office/2006/xmlPackage",
            w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

        public ProtocolFileParser(string fileName)
        {
            this.fileName = fileName;
        }

        public Protocol Parse(KnessetContext context)
        {
            PrepareHashsets(context); // prefetch some existing objects from the DB so that we can check quickly of objects are new or existing

            var ret = new Protocol();
            XDocument doc = XDocument.Load(fileName); // load the xml document using linq2xml

            // find the header element of the document
            var docHeader = (from e in doc.Document.Root.Elements(pkg + "part")
                             where e.Attribute(pkg + "name").Value == "/word/header2.xml"
                             select e.Element(pkg + "xmlData").Element(w + "hdr").Elements()
                             .Skip(1) // skip page number
                             .Take(1).ToArray() // take title
                             ).First();

            // find the body
            var docBody = (from e in doc.Document.Root.Elements(pkg + "part")
                           where e.Attribute(pkg + "name").Value == "/word/document.xml"
                           select e.Element(pkg + "xmlData").Element(w + "document").Element(w + "body")).First();

            // parse some protocol metadata from the header
            ParseHeaderMetadata(ret, docHeader);

            // parse rest of metadata and read all the content from the body
            ParseBody(ret, docBody, context);

            return ret;
        }

        // parse rest of metadata and read all the content from the body
        private void ParseBody(Protocol ret, XElement docBody, KnessetContext context)
        {
            var state = ProtocolState.InitialScan; // uae a state machine for different protocol parts
            bool isInvitationsTableRtl = false;
            Person currentSpeaker = null;
            foreach (XElement el in docBody.Elements())
            {
                switch (state)
                {
                    case ProtocolState.InitialScan:
                        if (IsCustomXml(el, "פרוטוקול"))
                        {
                            // try to read the protocol number from the custom XML
                            ret.pr_number = int.Parse(el.Element(w + "customXmlPr").Elements(w + "attr").First(x => x.Attribute(w + "name").Value == "Num").Attribute(w + "val").Value);
                            if (ret.pr_number == 0)
                            { // if the protocol number is not in the custom XML, read from the protocol title
                                int tmp;
                                // handle the last number in the protocol title as the protocol number
                                string pNumString = (from r in el.Element(w + "p").Elements(w + "r")
                                                     let str = r.Element(w + "t").Value.Trim()
                                                     where Regex.IsMatch(str, "^\\d+$")
                                                     select str).LastOrDefault() ?? "";
                                if (int.TryParse(Regex.Replace(pNumString, "[^\\d]", ""), out tmp))
                                    ret.pr_number = tmp;
                                // else we have a protocol without a number (?)
                            }
                            // read the protocol date from the custom XML attribute.
                            ret.pr_date = DateTime.ParseExact(el.Element(w + "customXmlPr").Elements(w + "attr").First(x => x.Attribute(w + "name").Value == "Date").Attribute(w + "val").Value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            // the next state is some protocol info
                            state = ProtocolState.ProtocolInfo;
                        }
                        break;
                    case ProtocolState.ProtocolInfo:
                        if (IsCustomXml(el, "סדר_יום")) // read info until we read the Agenda
                            state = ProtocolState.Agenda;
                        break;
                    case ProtocolState.Agenda:
                        if (IsSubject(el))
                        {
                            // we've reached the subject but we'll have another subject so we ignore this one
                            state = ProtocolState.SubjectLong;
                        }
                        break;
                    case ProtocolState.SubjectLong:
                        // ignore everything until we reach the list of members that are present
                        if (ContainsCustomXml(el, "חברי_הוועדה"))
                            state = ProtocolState.CommitteeMembers;
                        break;
                    case ProtocolState.CommitteeMembers:
                        // read the list of members that are present until we reach the invitations part
                        if (el.Name == w + "p")
                        {
                            if (ContainsCustomXml(el, "מוזמנים"))
                            {
                                // finished presence and now moving on to the invitations list
                                state = ProtocolState.Invitations;
                                break;
                            }
                            else if (IsSubject(el))
                            {
                                // no invitations
                                state = ReadSubject(ret, el);
                                break;
                            }
                            string content = ReadParagraph(el);
                            if (string.IsNullOrWhiteSpace(content) || content == "חברי הכנסת:")
                                break;
                            if (content.Contains("מוזמנים:"))
                            {
                                // in some rare cases the invitations title is not marked by custom XML and we need to check the text for it.
                                state = ProtocolState.Invitations;
                                break;
                            }
                            // if this is not a special line we got a presence to add.
                            var person = FindOrAddPerson(content);
                            if (!newPresence.Any(x=>x.person==person))
                            newPresence.Add(new Presence { person = person, protocol = ret });
                        }
                        break;
                    case ProtocolState.Invitations:
                        if (el.Name == w + "tbl")
                        {
                            // invitations of non-committee members are stored in a table
                            // if table is from right to left the first column has the name of the person
                            // else the last column (3rd)
                            isInvitationsTableRtl = el.Descendants(w + "bidiVisual").Any();
                            var items = from tr in el.Elements(w + "tr")
                                        select ReadParagraph((isInvitationsTableRtl ? tr.Elements(w + "tc").First() : tr.Elements(w + "tc").Last()).Element(w + "p"));
                            foreach (var invitation in items.Distinct())
                                newInvitations.Add(new Invitation { person = FindOrAddPerson(invitation), protocol = ret });
                        }
                        else if (IsSubject(el))
                        {
                            // after the invitations table we go on reading until we reach the header before the protocol starts
                            // no we read the protocol title.
                            state = ReadSubject(ret, el);
                        }
                        break;
                    case ProtocolState.Subject:
                        // after the subject heading we expect the actual protocol to start.
                        state = ProtocolState.Talking;
                        break;
                    case ProtocolState.Talking:
                        if (currentSpeaker != null && el.Name == w + "p")
                        {
                            // if we're in a 'normal' paragraph and we have a speaker then we the parahraph with the speaker.
                            string paragraphContent = ReadParagraph(el);
                            AddParagraph(ret, currentSpeaker, paragraphContent, context);
                        }
                        else if (IsCustomXml(el, "יור") || IsCustomXml(el, "דובר") || IsCustomXml(el, "דובר_המשך") || IsCustomXml(el, "אורח"))
                        {
                            // this is a new speaker starting to talk
                            string sprekerName = ReadParagraph(el.Element(w + "p"));
                            if (sprekerName.EndsWith(":"))
                                sprekerName = sprekerName.Substring(0, sprekerName.Length - 1);
                            currentSpeaker = FindOrAddPerson(sprekerName);
                        }
                        else if (IsCustomXml(el, "קריאה"))
                        {
                            // sometimes people make indistinguishable noises, just ignore...
                            currentSpeaker = null;
                        }
                        else if (IsCustomXml(el, "סיום"))
                        {
                            // we've reached the end of the protocol.
                            state = ProtocolState.Finished;
                        }
                        break;
                    case ProtocolState.Finished:
                        break;
                }
            }
            if (state != ProtocolState.Finished && newParagraphs.Count == 0)
            {
                throw new Exception(string.Format("Protocol parsing failed, did not pass state {0}", state));
            }
        }

        private ProtocolState ReadSubject(Protocol ret, XElement el)
        {
            ProtocolState state;
            var titleElem = el.Element(w + "customXml");
            if (titleElem.Element(w + "customXml") != null)
                titleElem = titleElem.Element(w + "customXml");
            ret.pr_title = ReadParagraph(titleElem);
            if (ret.pr_title.Length > 200)
                ret.pr_title = ret.pr_title.Substring(0, 200);
            state = ProtocolState.Subject;
            return state;
        }

        private enum ProtocolState
        {
            InitialScan,
            ProtocolInfo,
            Agenda,
            SubjectLong,
            CommitteeMembers,
            Invitations,
            Subject,
            Talking,
            Finished
        }

        // helpers:

        //      parse paragraph words and fillers, convert text to objects with word offsets, etc...
        private void AddParagraph(Protocol protocol, Person speaker, string paragraphContent, KnessetContext context)
        {
            if (string.IsNullOrWhiteSpace(paragraphContent))
            {
                return;
            }
            if (!speakerParahraphs.ContainsKey(speaker.pn_name)) speakerParahraphs.Add(speaker.pn_name, 0);
            Paragraph p = new Paragraph
            {
                protocol = protocol,
                speaker = speaker,
                pg_number = newParagraphs.Count + 1,
                pn_pg_number = ++speakerParahraphs[speaker.pn_name]
            };
            newParagraphs.Add(p);
            int offset = 0;
            StringBuilder fillBuilder = new StringBuilder();
            paragraphReader.Read(paragraphContent,
                word =>
                {
                    Word wordObj;
                    if (!existingWords.ContainsKey(word))
                    {
                        wordObj = new Word { word = word };
                        newWords.Add(wordObj);
                        existingWords.Add(word, wordObj);
                    }
                    else
                    {
                        wordObj = existingWords[word];
                    }
                    newParagraphWords.Add(new ParagraphWord
                    {
                        paragraph = p,
                        WordObj = wordObj,
                        pg_offset = offset,
                        word_number = p.pg_num_words++
                    });
                    offset += word.Length;
                },
                filler =>
                {
                    fillBuilder.Append(filler);
                    offset += filler.Length;
                });
            p.pg_space_fillers = fillBuilder.ToString();
        }

        //      a helper method to detect subject custom xml tag.
        private bool IsSubject(XElement el)
        {
            return ContainsCustomXml(el, "נושא") || ContainsCustomXml(el, "הצח") || ContainsCustomXml(el, "הלסי");
        }

        //      preload db objects so we don't try to add them again by mistake and so we can reference them from new objects.
        private void PrepareHashsets(KnessetContext context)
        {
            existingPersons = context.Persons.ToDictionary(x => x.pn_name);
            existingWords = context.Words.ToDictionary(x => x.word);
        }

        //  find person by name if already exists, else create a new person 
        private Person FindOrAddPerson(string name)
        {
            // some measured to prevent duplicate names - remove titles, parties, trim, etc...
            name = Regex.Replace(name, "\\([^\\)]*\\)", "");
            name = Regex.Replace(name, "[\\-–:'\"]", "");
            name = Regex.Replace(name, "(\\s|^)היור(\\s|$)", " ");
            name = Regex.Replace(name, "\\s+", " ").Trim();
            if (name.Length > 50) name = name.Substring(0, 50);
            if (!existingPersons.ContainsKey(name))
            {
                Person p = new Person { pn_name = name };
                existingPersons.Add(name, p);
                newPersons.Add(p);
                return p;
            }
            return existingPersons[name];
        }

        //      parse metadata from the document header, currently only the committee name
        private static void ParseHeaderMetadata(Protocol ret, XElement[] docHeader)
        {
            ret.c_name = docHeader[0].Element(w + "r").Element(w + "t").Value.Trim();
            if (ret.c_name.Length > 45)
                throw new Exception("Committee name unsupported (>45 chars)");
        }

        //      a small helper to read all the text parts of a paragraph (or elements with the same stracture or elem>w:r>w:t>text)
        //      and combine them to a single string
        public string ReadParagraph(XElement p)
        {
            return string.Join("", (from e in p.Elements(w + "r")
                                    let wt = e.Element(w + "t")
                                    select wt != null ? wt.Value : ""));
        }

        //      check if an element represents custom xml data of a specific type
        public bool IsCustomXml(XElement el, string requiredElement)
        {
            return (el.Name == w + "customXml") && (el.Attribute(w + "element").Value == requiredElement);
        }

        //      check if an element or any of its child elements represents custom xml data of a specific type
        public bool ContainsCustomXml(XElement el, string requiredElement)
        {
            if (IsCustomXml(el, requiredElement)) return true;
            return el.Elements(w + "customXml").Any(x => x.Attribute(w + "element").Value == requiredElement);
        }
    }
}