using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace SigningCheck
{
    class SigcheckHelper
    {
        private StringBuilder outputlog;
        private ProcessStartInfo startInfo;
        public SigcheckHelper()
        {
            outputlog = new StringBuilder();
            startInfo = new ProcessStartInfo
            {
                FileName = "exe\\sigcheck.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WindowStyle = ProcessWindowStyle.Minimized,
                Verb = "runas"
            };
        }

        public Task<string> DumpContent(string fileName)
        {
            outputlog.Clear();

            return Task.Run(() =>
            {
                startInfo.Arguments = "/accepteula -d -s " + "\"" + fileName + "\"";
                Process getDump = new Process();
                getDump.StartInfo = startInfo;

                ExecuteProc(getDump);

                return outputlog.ToString();
            });
        }

        public Task<string> GetSiningChain(string fileName)
        {
            outputlog.Clear();

            return Task.Run(() =>
            {
                startInfo.Arguments = "/accepteula -i -s " + "\"" + fileName + "\"";
                Process getChain = new Process();
                getChain.StartInfo = startInfo;

                ExecuteProc(getChain);

                return outputlog.ToString();
            });
        }

        private void ExecuteProc(Process process)
        {
            process.Start();
            process.OutputDataReceived += Process_OutputDataReceived;
            process.ErrorDataReceived += Process_ErrorDataReceived;

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            process.Close();
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.Error.WriteLine(e.Data);
            return;
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
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
