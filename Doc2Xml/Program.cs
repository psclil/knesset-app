using Microsoft.Office.Interop.Word;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text.RegularExpressions;

namespace Doc2Xml
{
    /**
     * A program to convert doc/x files to standalone open xml format that can be processed by other apps
     * Usage: pass list of word file paths as argumets, if no arguments are passed all doc files in the current
     *        directory will be converted.
     **/
    class Program
    {
        static void Main(string[] args)
        {
            WdSaveFormat toFormat = WdSaveFormat.wdFormatFlatXML;
            string toExt = ".xml";

            List<string> fileNames = new List<string>(args);

            if (fileNames.Contains("--pdf", StringComparer.InvariantCultureIgnoreCase))
            {
                toFormat = WdSaveFormat.wdFormatPDF;
                toExt = ".pdf";
            }
            else if (fileNames.Contains("--rtf", StringComparer.InvariantCultureIgnoreCase))
            {
                toFormat = WdSaveFormat.wdFormatRTF;
                toExt = ".rtf";
            }
            else if (fileNames.Contains("--txt", StringComparer.InvariantCultureIgnoreCase))
            {
                toFormat = WdSaveFormat.wdFormatUnicodeText;
                toExt = ".txt";
            }
            // if files were passed convert them to full path (because the word app does not start in the current dir)
            fileNames = (from f in fileNames
                         where File.Exists(f)
                         let fo = new FileInfo(f)
                         select fo.FullName).ToList();
            if (fileNames.Count == 0)
            {
                // if no files passed in args iterate all doc files in the current directory
                DirectoryInfo d = new DirectoryInfo(Directory.GetCurrentDirectory());
                fileNames = d.GetFiles("*.doc").Select(x => x.FullName).ToList(); // .doc also matched .docx
            }
            // create a new instance of the Word app.
            var app = new Application();
            foreach (string fname in fileNames)
            {
                try
                {
                    Console.Error.Write(fname);
                    string newFileName = Regex.Replace(fname, "\\.[a-z0-9]+", "", RegexOptions.IgnoreCase) + toExt;
                    if (!File.Exists(newFileName))
                    {
                        Document d = app.Documents.Open(FileName: fname, ReadOnly: true);
                        object fileFormat = toFormat;
                        d.SaveAs2(FileName: newFileName, FileFormat: fileFormat);
                        d.Close(WdSaveOptions.wdDoNotSaveChanges);
                    }
                    Console.Error.WriteLine(" --> {0}", newFileName);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("-----------------------------------------");
                    Console.Error.WriteLine("*****************************************");
                    Console.Error.WriteLine("-----------------------------------------");
                    Console.Error.WriteLine(fname);
                    Console.Error.WriteLine("Error processing file:");
                    Console.Error.WriteLine(ex);
                    Console.Error.WriteLine("-----------------------------------------");
                    Console.Error.WriteLine("*****************************************");
                    Console.Error.WriteLine("-----------------------------------------");
                }
            }
            // close the app
            app.Quit(WdSaveOptions.wdDoNotSaveChanges);
        }
    }
}
