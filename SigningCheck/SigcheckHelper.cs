using System.Diagnostics;
using System.Threading.Tasks;

namespace SigningCheck
{
    class SigcheckHelper : ExeHelper
    {
        public SigcheckHelper() : base("exe\\sigcheck.exe") { }

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
    }
}
