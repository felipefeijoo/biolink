using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.IO;
using BioLink.Client.Utilities;


namespace BioLinkDALGenerator {

    class Program {

        private string server = "biolinkdev-w7\\sqlexpress";
        private string database = "BiolinkDemo";
        private string username = "sa";
        private string password = "ants";
        private string dbmlfile = "biolink.dbml";

        private static List<string> TYPE_PREFIX_LIST = new List<string>();

        static Program() {
            TYPE_PREFIX_LIST.Add("Int");
            TYPE_PREFIX_LIST.Add("Vchr");
            TYPE_PREFIX_LIST.Add("Dt");
            TYPE_PREFIX_LIST.Add("Bit");
            TYPE_PREFIX_LIST.Add("Txt");
        }

        static void Main(string[] args) {
            try {
                new Program().TestIt();
            } catch (Exception ex) {
                Console.WriteLine(ex);
            }

            Console.ReadKey();

        }

        private void TestIt() {
            IniFile f = new IniFile();
            f.Load("c:/zz/test.ini");

            Console.WriteLine(f.GetValue("UNO - Dallwitz", "HOST"));

            var writer = new StringWriter();
            f.Write(writer);

            Console.WriteLine(writer.ToString());
        }

        private void DoIt() {
            string arguments = String.Format("/server:{0} /database:{1} /user:{2} /password:{3} /sprocs /dbml:{4}", server, database, username, password, dbmlfile);
            log("Launching sqlmetal...");
            Process process = Process.Start("C:/Program Files/Microsoft SDKs/Windows/v7.0A/bin/sqlmetal.exe", arguments);
            log("Waiting...");
            process.WaitForExit();
            log("Parsing DBML...");

            XmlDocument doc = new XmlDocument();
            doc.Load(dbmlfile);
            log("Processing tables...");
            ProcessTables(doc);
            log("Writing file...");
            doc.Save(dbmlfile);
            log("Finished.");
        }

        private XmlNodeList SelectNodes(XmlElement element, string xpath) {
            XmlNamespaceManager xmlnsmgr = new XmlNamespaceManager(element.OwnerDocument.NameTable);
            xmlnsmgr.AddNamespace("l", "http://schemas.microsoft.com/linqtosql/dbml/2007");
            XmlNodeList nl = element.SelectNodes(xpath, xmlnsmgr);
            return nl;
        }

        private void ProcessTables(XmlDocument doc) {
            XmlNodeList nl = SelectNodes(doc.DocumentElement, "/l:Database/l:Table");

            foreach (XmlElement tableElement in nl) {                
                string name = tableElement.GetAttribute("Name");
                String member = tableElement.GetAttribute("Member");
                log("Processing table: {0}", name);
                if (member.StartsWith("Tbl")) {
                    tableElement.SetAttribute("Member", member.Substring(3));
                }
                ProcessTable(tableElement);
            }
        }

        private void ProcessTable(XmlElement tableElement) {
            // Fix the type name...
            XmlElement typeElement = SelectNodes(tableElement, "l:Type").Item(0) as XmlElement;
            if (typeElement != null) {
                string typeName = typeElement.GetAttribute("Name");
                if (typeName.StartsWith("Tbl")) {
                    String newname = "T" + typeName.Substring(3);
                    log("Fixing type: {0} -> {1}", typeName, newname);
                    typeElement.SetAttribute("Name", newname);
                }
            }

            XmlNodeList nl = SelectNodes(tableElement, "l:Type/l:Column");

            foreach (XmlElement columnElement in nl) {
                string member = columnElement.GetAttribute("Member");
                foreach (String prefix in TYPE_PREFIX_LIST) {
                    if (member.StartsWith(prefix)) {
                        member = member.Substring(prefix.Length);
                        log("  Fixing member: {0}", member);
                        columnElement.SetAttribute("Member", member);
                    }
                }
            }

        }

        private static void log(string message, params object[] args) {
            String formatted = String.Format(message, args);
            Console.Out.WriteLine(formatted);
        }
    }
}
