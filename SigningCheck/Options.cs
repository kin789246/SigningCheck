using System;
using System.IO;
using System.Text.RegularExpressions;

namespace SigningCheck
{
    public class Options
    {
        private bool logByDir = false;
        private bool isZip;
        private string sourceName;
        private string outputName;

        public bool LogByDir { get { return logByDir; } }
        public bool IsZip { get { return isZip; } }
        public string SourceName { get { return sourceName; } }
        public string OutputName { get { return outputName; } }

        public void Build(string[] args)
        {
            int i = 0;
            while (i < args.Length)
            {
                if (args[i] == "-f")
                {
                    logByDir = true;
                }
                if (args[i] == "-p")
                {
                    if (i + 1 < args.Length) 
                    {
                        i++;
                        if (args[i].StartsWith("-"))
                        {
                            return;
                        }
                        sourceName = args[i];
                        if (Path.GetExtension(args[i]) == ".zip")
                        {
                            isZip = true;
                            outputName = Path.GetFileNameWithoutExtension(args[i]) + DateTime.Now.ToString("_yyyyMMdd_HHmmss");
                        }
                        else
                        {
                            isZip = false;
                            string rgxDrvPath = @"\\.+\\";
                            outputName = args[i].TrimEnd('\\');
                            Match match = Regex.Match(outputName, rgxDrvPath);
                            if (match.Success)
                            {
                                outputName = outputName.Substring(match.Index + match.Length);
                            }
                            else
                            {
                                outputName = outputName.Substring(args[i].IndexOf('\\') + 1);
                            }
                            outputName = outputName + DateTime.Now.ToString("_yyyyMMdd_HHmmss");
                        }
                    }
                }
                i++;
            }

            if (logByDir)
            {
                outputName = outputName + "\\" + outputName;
            }
        }
    }
}
