using System.Diagnostics;
using System.Threading.Tasks;

namespace SigningCheck
{
    public class ExpandHelper : ExeHelper
    {
        public ExpandHelper() : base("expand.exe") { }
        public Task<string> ExpandFile(string source, string destination)
        {
            outputlog.Clear();

            return Task.Run(() =>
            {
                startInfo.Arguments = "\"" + source + "\"" + " \"" + destination +"\"";
                Process getDump = new Process();
                getDump.StartInfo = startInfo;

                ExecuteProc(getDump);

                return outputlog.ToString();
            });
        }
    }
}
