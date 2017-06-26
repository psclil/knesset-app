using knesset_app.DBEntities;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace knesset_app
{
    public class ProtocolFileParser
    {
        private static XNamespace
            pkg = "http://schemas.microsoft.com/office/2006/xmlPackage",
            w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

        protected string fileName;

        public ProtocolFileParser(string fileName)
        {
            this.fileName = fileName;
        }

        public string ReadParagraph(XElement p)
        {
            return string.Join("", (from e in p.Elements(w + "r")
                                    select e.Element(w + "t").Value));
        }

        public Protocol Parse(KnessetContext context)
        {
            var ret = new Protocol();
            XDocument doc = XDocument.Load(fileName);

            var docHeader = (from e in doc.Document.Root.Elements(pkg + "part")
                             where e.Attribute(pkg + "name").Value == "/word/header2.xml"
                             select e.Element(pkg + "xmlData").Element(w + "hdr").Elements()
                             .Skip(1) // skip page number
                             .Take(2).ToArray() // take title and date
                             ).First();

            ParseHeaderMetadata(ret, docHeader);

            var docBody = (from e in doc.Document.Root.Elements(pkg + "part")
                           where e.Attribute(pkg + "name").Value == "/word/document.xml"
                           select e.Element(pkg + "xmlData").Element(w + "document").Element(w + "body")).First();

            ParseBodyMetadata(ret, docBody);

            return ret;
        }

        private void ParseBodyMetadata(Protocol ret, XElement docBody)
        {
            ret.pr_title = (from e in docBody.Elements(w + "customXml")
                            where e.Attribute(w + "element").Value == "נושא"
                            select ReadParagraph(e.Element(w + "p"))).First();

            ret.pr_number = (from e in docBody.Elements(w + "customXml")
                             where e.Attribute(w + "element").Value == "פרוטוקול"
                             select int.Parse(e.Element(w + "p").Elements(w + "r").Last().Element(w + "t").Value)).First();
        }

        private static void ParseHeaderMetadata(Protocol ret, XElement[] docHeader)
        {
            ret.c_name = docHeader[0].Element(w + "r").Element(w + "t").Value;
            ret.pr_date = DateTime.ParseExact(docHeader[1].Element(w + "r").Element(w + "t").Value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
        }
    }
}