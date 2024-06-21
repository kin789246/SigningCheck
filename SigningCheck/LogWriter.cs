using System;
using System.IO;

namespace SigningCheck
{
    public static class LogWriter
    {
        public static void Log(string logMessage, string logName, bool addTime = true)
        {
            string logDir = "logs";
            string logFullPath = Path.Combine(logDir, logName); 
            logDir = Path.GetDirectoryName(logFullPath);
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            using (TextWriter txtWriter = File.AppendText(logFullPath))
            {
                try
                {
                    if (addTime)
                    {
                        txtWriter.Write("{0} : ", DateTime.Now.ToString("yyyy-MM-dd_hh:mm:ss"));
                    }
                    txtWriter.WriteLine(logMessage);
                }
                catch (Exception ex)
                {
                    // Handle exceptions if needed
                    Console.Error.WriteLine(ex.ToString());
                }
            }
        }
    }
}
