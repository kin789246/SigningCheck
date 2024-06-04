using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace SigningCheck
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine(Version);
            if (args.Length != 1)
            {
                Console.Write("need file name");
                return;
            }
            string zipName = args[0];
            if (Path.GetExtension(zipName) != ".zip")
            {
                Console.Write($"{Path.GetFileName(zipName)} Not a zip file");
                return;
            }
            string logName = "signDrvCheck" + DateTime.Now.ToString("_yyyyMMdd_HHmmss") + ".log";
            string resultName = Path.GetFileNameWithoutExtension(zipName) + DateTime.Now.ToString("_yyyyMMdd_HHmmss");
            //extract zip file
            string extractPath = "extract";
            ZipHelper zh = new ZipHelper(extractPath, zipName, logName);
            Log("start extracting " + zipName, logName);
            zh.ExtractFiles(new List<string> { "cat", "dll", "sys" });
            //check if any file has been exact
            if (Directory.Exists(extractPath))
            {
                Log("start parsing", logName);
                SigcheckHelper sigcheckHelper = new SigcheckHelper();
                List<SigcheckData> sigcheckDatas = new List<SigcheckData>();

                //get dump of cat files
                Log("get dump for cat filels", logName);
                string dumplog = await sigcheckHelper.DumpContent(extractPath + "\\*.cat");
                string dumpName = Path.GetFileNameWithoutExtension(zipName) +
                    "_dump" + DateTime.Now.ToString("_yyyyMMdd_HHmmss") + ".log";
                Log(dumplog, dumpName, false);
                Log("dump is saved to " + dumpName, logName);
                Log("parsing dump", logName);
                DumpParser.ParseDump(dumplog, sigcheckDatas, extractPath);

                //get signer chain of cat, sys, dll files
                Log("get signing chain", logName);
                string signingChain = await sigcheckHelper.GetSiningChain(extractPath + "\\*.*");
                string chainName = Path.GetFileNameWithoutExtension(zipName)
                    + "_signingChain" + DateTime.Now.ToString("_yyyyMMdd_HHmmss") + ".log";
                Log(signingChain, chainName, false);
                Log("signing chain information is saved to " + chainName, logName);
                Log("parsing signing chain information", logName);
                SigningChainParser.ParseSigningChain(signingChain, sigcheckDatas, extractPath);

                generateCSV(resultName, sigcheckDatas);
                Log("Summary is saved to " + resultName + ".csv and " + resultName + ".html", logName, true, true);

                //delete the extract directory
                Directory.Delete(extractPath, true);
            }
            else
            {
                Log("Can't find cat, dll, sys files in this zip.", logName, true, true);
            }

            Log("### end ###", logName);
        }
        private static string Version
        {
            get
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                AssemblyName asmName = asm.GetName();
                return String.Format("{0} v{1} by Kin|Jiaching", asmName.Name, asmName.Version.ToString());
            }
        }
        private static void generateCSV(string fileName, List<SigcheckData> sigcheckDatas)
        {
            CsvOutData csvOutData = new CsvOutData();
            string osCfg = "config\\os.cfg";
            List<(string, bool)> osVersion = new List<(string, bool)>();
            loadOsCfg(osCfg, osVersion);
            csvOutData.GetTitle(osVersion);
            foreach (var item in sigcheckDatas)
            {
                CsvData output = new CsvData(item, osVersion);
                output.GenerateOutput();
                csvOutData.Data.Add(output);
            }
            
            Log(csvOutData.ToCsvString(), fileName + ".csv", false);
            Log(HtmlHelper.ToHtmlTable(csvOutData), fileName + ".html", false);
        }

        private static void loadOsCfg(string osCfgFile, List<(string, bool)> osVersion)
        {
            using (StreamReader sr = new StreamReader(osCfgFile))
            {
                try
                {
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        osVersion.Add((line.ToUpper(), false));
                        line = sr.ReadLine();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        private static void Log(string message, string fileName, bool timeStamp = true, bool screen = false)
        {
            if (screen)
            {
                Console.WriteLine(message);
            }
            LogWriter.Log(message, fileName, timeStamp);
        }
    }
}
