using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SigningCheck
{
    internal static class DumpParser
    {
        internal static void ParseDump(string raw, List<SigcheckData> sigcheckDatas, string extractPath)
        {
            StringReader sr = new StringReader(raw);
            string line = sr.ReadLine();
            string rgxPath = @"^[a-zA-Z]\:\\.+\.(cat|dll|sys)";
            string rgxOS = @"OS: *";
            string rgxType = @"SigningType: *";

            SigcheckData sc = new SigcheckData();
            while (!string.IsNullOrEmpty(line))
            {
                if (Regex.IsMatch(line, rgxPath))
                {
                    sc = new SigcheckData();
                    sigcheckDatas.Add(sc);
                    sc.FileName = line;
                    Match match = Regex.Match(line, extractPath);
                    if (match.Success)
                    {
                        sc.FileName = line.Substring(match.Index + extractPath.Length);
                    }
                }
                else if (Regex.IsMatch(line, rgxOS))
                {
                    string os = "OS: ";
                    Match match = Regex.Match(line, os);
                    if (match.Success)
                    {
                        sc.OsSupport = line.Substring(match.Index + os.Length);
                    }
                }
                else if (Regex.IsMatch(line, rgxType))
                {
                    string st = "SigningType: ";
                    Match match = Regex.Match(line, st);
                    if (match.Success)
                    {
                        sc.SigningType = line.Substring(match.Index + st.Length);
                    }
                }
                line = sr.ReadLine();
            }
        }
    }
}


//string rgx = @"OS: *";
//if (Regex.IsMatch(e.Data, rgx))
//{
//    outputlog.Append(e.Data).AppendLine();
//}

//SigningType: PreProd

/*
    dump cat: sigcheck.exe -d -s *.cat

    D:\catdllsys\files\intcpmt.cat
    HWID1: pci\ven_8086&dev_7d0d
    HWID2: pci\ven_8086&dev_ad0d
    OS: _v100,_v100_X64,Server_v100_X64,Server_v100_ARM64
    SigningType: PreProd
    PackageId: 3daf5a2b-748c-44d8-a621-45d25a3a4058
    BundleID: Driver
    Submission ID:  

    D:\catdllsys\files\ishheciextensiontemplate_att.cat
    HWID1: {95511210-d1f0-4091-b373-46fdcc5329f7}\ish_heci
    OS: _v100_X64_21H2,_v100_X64_22H2,_v100_X64_24H2
    Declarative: True
    Universal: False
    BundleID: ISH
    Submission ID: 29990010_14368141040126453_1152921505697749527
 */

