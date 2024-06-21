using System;
using System.Diagnostics;
using System.Text;

namespace SigningCheck
{
    public class ExeHelper
    {
        protected StringBuilder outputlog = new StringBuilder();
        protected ProcessStartInfo startInfo;
        public ExeHelper(string name)
        {
            startInfo = new ProcessStartInfo
            {
                FileName = name,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Verb = "runas"
            };
        }

        protected void ExecuteProc(Process process)
        {
            process.Start();
            process.OutputDataReceived += Process_OutputDataReceived;
            process.ErrorDataReceived += Process_ErrorDataReceived;

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            process.Close();
        }

        protected void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.Error.WriteLine(e.Data);
        }

        protected void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                lock (outputlog)
                {
                    outputlog.Append(e.Data).AppendLine();
                }
            }
        }
    }
}
