using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SigningCheck
{
    internal class Program
    {
        private static string logName;
        private static Options opts;
        static async Task<int> Main(string[] args)
        {
            Console.WriteLine(Version + " by Kin|Jiaching");
            opts = parseParameter(args);
            Log("### " + Version + " ###", logName);
            List<SigcheckData> sigcheckDatas = new List<SigcheckData>();
            if (opts.IsZip)
            {
                string extractPath = "tmp";
                ZipHelper zh = new ZipHelper(extractPath, opts.SourceName, logName);
                Log("start extracting " + opts.SourceName, logName);
                zh.ExtractFiles(new List<string> { "cat", "dll", "sys" });
                await processFiles(extractPath, sigcheckDatas, opts.OutputName, logName);

                if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);
            }
            else
            {
                await processFiles(opts.SourceName, sigcheckDatas, opts.OutputName, logName);
            }

            Log("### end ###", logName);
            return 0;
        }
        private static Options parseParameter(string[] args)
        {
            Options options = new Options();
            string helpStr =
                    "command examples:\n" +
                    "command1: signingcheck -p path\\drv_name.zip\n" +
                    "command2: signingcheck -p path\\of\\driver\\directory\n\n" +
                    "add -f for logs in folder\n" +
                    "command3: signingcheck -f -p path\\drv_name.zip\n" +
                    "command4: signingcheck -f -p path\\of\\driver\\directory";
            if (args.Length < 1)
            {
                Console.WriteLine(helpStr);
                Environment.Exit(1);
            }
            options.Build(args);
            if (string.IsNullOrEmpty(options.SourceName))
            {
                Console.WriteLine(helpStr);
                Environment.Exit(1);
            }
            logName = "signDrvCheck" + DateTime.Now.ToString("_yyyyMMdd_HHmmss") + ".log";
            return options;
        }
        private async static Task processFiles(string drvPath, List<SigcheckData> sigcheckDatas, string resultName, string logName)
        {
            drvPath = Path.GetFullPath(drvPath);
            if (Directory.Exists(drvPath))
            {
                Log("start parsing", logName);
                SigcheckHelper sigcheckHelper = new SigcheckHelper();

                Log("get dump for cat filels", logName);
                string dumplog = await sigcheckHelper.DumpContent(drvPath + "\\*.cat");
                string dumpName = resultName + "_dump.log";
                Log(dumplog, dumpName, false);
                Log("dump is saved to " + dumpName, logName);
                Log("parsing dump", logName);
                DumpParser.ParseDump(dumplog, sigcheckDatas, drvPath);

                Log("get signing chain", logName);
                await getSigningChain(new List<string> { "cat", "dll", "sys" }, sigcheckDatas, drvPath, resultName, logName);

                generateCSV(resultName, sigcheckDatas);
                Log("Summary is saved to " + resultName + ".csv and " + resultName + ".html", logName, true, true);
            }
            else
            {
                Log("Can't find cat, dll, sys files in this driver package.", logName, true, true);
            }
        }
        private async static Task getSigningChain(List<string> extensions, List<SigcheckData> sigcheckDatas, string drvPath, string resultName, string logName)
        {
            foreach (var ext in extensions)
            {
                SigcheckHelper sigcheckHelper = new SigcheckHelper();
                string signingChain = await sigcheckHelper.GetSiningChain(drvPath + "\\*." + ext);
                string chainName = resultName + "_signingChain.log";
                Log(signingChain, chainName, false);
                Log(ext + " files signing chain information is saved to " + chainName, logName);
                Log("parsing signing chain information for " + ext + " files", logName);
                SigningChainParser.ParseSigningChain(signingChain, sigcheckDatas, drvPath);
            }
        }
        private static string Version
        {
            get
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                AssemblyName asmName = asm.GetName();
                return String.Format("{0} v{1}", asmName.Name, asmName.Version.ToString());
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

            //sort by file path
            csvOutData.Data = csvOutData.Data.OrderBy(x => x.FilePath).ToList<CsvData>();

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
