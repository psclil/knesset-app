using Microsoft.Office.Interop.Word;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System;

namespace Doc2Xml
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> fileNames = new List<string>(args);
            if (fileNames.Count == 0)
            {
                DirectoryInfo d = new DirectoryInfo(Directory.GetCurrentDirectory());
                fileNames = d.GetFiles("*.doc").Select(x => x.FullName).ToList();
            }
            else
            {
                fileNames = (from f in fileNames
                             let fo = new FileInfo(f)
                             select fo.FullName).ToList();
            }
            var app = new Application();
            foreach (string fname in fileNames)
            {
                try
                {
                    Console.Error.Write(fname);
                    Document d = app.Documents.Open(FileName: fname, ReadOnly: true);
                    object fileFormat = WdSaveFormat.wdFormatFlatXML;
                    object newFileName = fname.Replace(".doc", "") + ".xml";
                    d.SaveAs2(FileName: newFileName, FileFormat: fileFormat);
                    Console.Error.WriteLine(" --> {0}", newFileName);
                    d.Close(WdSaveOptions.wdDoNotSaveChanges);
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
            app.Quit(WdSaveOptions.wdDoNotSaveChanges);
        }
    }
}
