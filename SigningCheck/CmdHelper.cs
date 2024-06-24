using System.Diagnostics;
namespace SigningCheck
{
    public static class CmdHelper
    {
        public static string Run(string command)
        {
            string output = string.Empty;
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.Arguments = "/c " + command;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }
            return output;
        }
    }
}
