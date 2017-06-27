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
    public class ProtocolFileParser
    {
        protected string fileName;
        protected ParagraphReader paragraphReader = new ParagraphReader();

        private Dictionary<string, int> speakerParahraphs = new Dictionary<string, int>();
        private Dictionary<string, Person> existingPersons;
        private Dictionary<string, Word> existingWords;
        public List<Person> newPersons = new List<Person>();
        public List<Word> newWords = new List<Word>();
        public List<ParagraphWord> newParagraphWords = new List<ParagraphWord>();
        public List<Paragraph> newParagraphs = new List<Paragraph>();
        public List<Presence> newPresence = new List<Presence>();
        public List<Invitation> newInvitations = new List<Invitation>();

        private static XNamespace
            pkg = "http://schemas.microsoft.com/office/2006/xmlPackage",
            w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

        public ProtocolFileParser(string fileName)
        {
            this.fileName = fileName;
        }

        public string ReadParagraph(XElement p)
        {
            return string.Join("", (from e in p.Elements(w + "r")
                                    select e.Element(w + "t").Value)).Trim();
        }

        public bool IsCustomXml(XElement el, string requiredElement)
        {
            return (el.Name == w + "customXml") && (el.Attribute(w + "element").Value == requiredElement);
        }

        public bool ContainsCustomXml(XElement el, string requiredElement)
        {
            return el.Elements(w + "customXml").Any(x => x.Attribute(w + "element").Value == requiredElement);
        }

        public Protocol Parse(KnessetContext context)
        {
            PrepareHashsets(context);

            var ret = new Protocol();
            XDocument doc = XDocument.Load(fileName);

            var docHeader = (from e in doc.Document.Root.Elements(pkg + "part")
                             where e.Attribute(pkg + "name").Value == "/word/header2.xml"
                             select e.Element(pkg + "xmlData").Element(w + "hdr").Elements()
                             .Skip(1) // skip page number
                             .Take(1).ToArray() // take title
                             ).First();

            var docBody = (from e in doc.Document.Root.Elements(pkg + "part")
                           where e.Attribute(pkg + "name").Value == "/word/document.xml"
                           select e.Element(pkg + "xmlData").Element(w + "document").Element(w + "body")).First();

            ParseHeaderMetadata(ret, docHeader);

            ParseBody(ret, docBody, context);

            return ret;
        }

        private void PrepareHashsets(KnessetContext context)
        {
            existingPersons = context.Persons.ToDictionary(x => x.pn_name);
            existingWords = context.Words.ToDictionary(x => x.word);
        }

        private void ParseBody(Protocol ret, XElement docBody, KnessetContext context)
        {
            var state = ProtocolState.InitialScan;
            bool invitationsRtl = false;
            Person speaker = null;
            foreach (XElement el in docBody.Elements())
            {
                switch (state)
                {
                    case ProtocolState.InitialScan:
                        if (IsCustomXml(el, "פרוטוקול"))
                        {
                            state = ProtocolState.ProtocolInfo;
                            ret.pr_number = int.Parse(el.Element(w + "customXmlPr").Elements(w + "attr").First(x => x.Attribute(w + "name").Value == "Num").Attribute(w + "val").Value);
                            if (ret.pr_number == 0)
                                ret.pr_number = int.Parse(Regex.Replace(el.Element(w + "p").Elements(w + "r").Last().Element(w + "t").Value, "[^\\d]", ""));
                            ret.pr_date = DateTime.ParseExact(el.Element(w + "customXmlPr").Elements(w + "attr").First(x => x.Attribute(w + "name").Value == "Date").Attribute(w + "val").Value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        }
                        break;
                    case ProtocolState.ProtocolInfo:
                        if (IsCustomXml(el, "סדר_יום"))
                            state = ProtocolState.Agenda;
                        break;
                    case ProtocolState.Agenda:
                        if (IsCustomXml(el, "נושא"))
                        {
                            state = ProtocolState.SubjectLong;
                        }
                        break;
                    case ProtocolState.SubjectLong:
                        if (ContainsCustomXml(el, "חברי_הוועדה"))
                            state = ProtocolState.CommitteeMembers;
                        break;
                    case ProtocolState.CommitteeMembers:
                        if (el.Name == w + "p")
                        {
                            if (ContainsCustomXml(el, "מוזמנים"))
                            {
                                state = ProtocolState.Invitations;
                                break;
                            }
                            string content = ReadParagraph(el);
                            if (string.IsNullOrWhiteSpace(content) || content == "חברי הכנסת:")
                                break;
                            AddPersence(ret, context, content);
                        }
                        break;
                    case ProtocolState.Invitations:
                        if (el.Name == w + "tbl")
                        {
                            invitationsRtl = el.Descendants(w + "bidiVisual").Any();
                            var items = from tr in el.Elements(w + "tr")
                                        select ReadParagraph((invitationsRtl ? tr.Elements(w + "tc").First() : tr.Elements(w + "tc").Last()).Element(w + "p"));
                            foreach (var invitation in items)
                                AddInvitation(ret, context, invitation);
                        }
                        else if (ContainsCustomXml(el, "נושא"))
                        {
                            ret.pr_title = ReadParagraph(el.Element(w + "customXml"));
                            state = ProtocolState.Subject;
                        }
                        break;
                    case ProtocolState.Subject:
                        state = ProtocolState.Talking;
                        break;
                    case ProtocolState.Talking:
                        if (speaker != null && el.Name == w + "p")
                        {
                            string paragraphContent = ReadParagraph(el);
                            AddParagraph(ret, speaker, paragraphContent, context);
                        }
                        else if (IsCustomXml(el, "יור") || IsCustomXml(el, "דובר") || IsCustomXml(el, "דובר_המשך") || IsCustomXml(el, "אורח"))
                        {
                            string sprekerName = ReadParagraph(el.Element(w + "p"));
                            if (sprekerName.EndsWith(":"))
                                sprekerName = sprekerName.Substring(0, sprekerName.Length - 1);
                            speaker = FindOrAddPerson(sprekerName);
                        }
                        else if (IsCustomXml(el, "קריאה"))
                        {
                            speaker = null;
                        }
                        else if (IsCustomXml(el, "סיום"))
                        {
                            state = ProtocolState.Finished;
                        }
                        break;
                    case ProtocolState.Finished:
                        break;
                }
            }
        }

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

        private void AddPersence(Protocol protocol, KnessetContext context, string name)
        {
            newPresence.Add(new Presence { person = FindOrAddPerson(name), protocol = protocol });
        }

        private void AddInvitation(Protocol protocol, KnessetContext context, string name)
        {
            newInvitations.Add(new Invitation { person = FindOrAddPerson(name), protocol = protocol });
        }

        private static void ParseHeaderMetadata(Protocol ret, XElement[] docHeader)
        {
            ret.c_name = docHeader[0].Element(w + "r").Element(w + "t").Value;
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
    }
}