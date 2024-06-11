using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SigningCheck
{
    public class CsvOutData
    {
        private string summary = string.Empty;
        private string title = string.Empty; 
        private List<CsvData> data = new List<CsvData>();
        public string Summary { get { return summary; } }
        public string Title { get { return title; } }
        public List<CsvData> Data { get { return data; } set { data = value; } }

        private void getSummary()
        {
            if (data.Count == 0)
            {
                summary = "No file is analyzed";
            }
            else if (data.All(csv => csv.Summary.Equals(data[0].Summary, StringComparison.OrdinalIgnoreCase)))
            {
                summary = data[0].Summary;
            }
            else
            {
                summary = "This driver package has different signed drivers. Please check below detail information.";
            }
        }
        public void GetTitle(List<(string, bool)> osVersion)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Name,Summary,Path,PreProd,Attestation,TestSigning,WHQL,");
            foreach (var item in osVersion)
            {
                sb.Append(item.Item1);
                sb.Append(',');
            }
            sb.Append("OtherOS,Expiry,Signers{Name||ValidUsages||SigningDate||ValidFrom||ValidTo}");
            title = sb.ToString();
        }

        public string ToCsvString()
        {
            StringBuilder sb = new StringBuilder();
            getSummary();
            sb.Append(summary).AppendLine();
            sb.Append(title).AppendLine();
            foreach (var item in data)
            {
                sb.Append(item.ToString()).AppendLine();
            }
            return sb.ToString();
        }
    }
    public class CsvData
    {
        private const string vuWHQL = "whql crypto";
        private const string vuAttestation = "1.3.6.1.4.1.311.10.3.5.1";
        private const string vuLifetime = "lifetime signing";
        private const string whcp = "Microsoft Windows Hardware Compatibility Publisher";

        private string name = string.Empty;
        private string summary = string.Empty;
        private string filePath = string.Empty;
        private bool preProd = false;
        private bool testSigning = false;
        private bool attestation = false;
        private bool whql = false;
        private List<(string, bool)> osversion;
        private string otherOS = string.Empty;
        private string signerInfo = string.Empty;
        private string tsExpiryDate = string.Empty;
        private SigcheckData sigcheckData;

        public string Summary { get { return summary; } }
        public string Name { get { return name; } }
        public string FilePath { get { return filePath; } }
        public bool PreProd { get { return preProd; } }
        public bool TestSigning { get { return testSigning; } }
        public bool Attestation { get { return attestation; } }
        public bool Whql { get { return whql; } }
        public List<(string, bool)> Osversion { get { return osversion; } }
        public string OtherOS { get { return otherOS; } }
        public string TsExpiryDate { get { return tsExpiryDate; } }
        public SigcheckData SigcheckData { get { return sigcheckData; } }
        
        public CsvData(SigcheckData sigcheckData, List<(string, bool)> osv)
        {
            this.sigcheckData = sigcheckData;
            this.osversion = new List<(string, bool)>(osv);
        }
        public void GenerateOutput()
        {
            name = Path.GetFileName(sigcheckData.FileName);
            filePath = Path.GetDirectoryName(sigcheckData.FileName);
            if (string.Equals(sigcheckData.SigningType, "PreProd", System.StringComparison.OrdinalIgnoreCase))
            {
                preProd = true;
            }

            foreach (var item in sigcheckData.OsSupport.Split(','))
            {
                bool noOtherOS = false;
                for (int i = 0; i < osversion.Count; i++)
                {
                    if (item.ToUpper().Contains(osversion[i].Item1))
                    {
                        osversion[i] = (osversion[i].Item1, true);
                        noOtherOS = true;
                    }
                }
                if (!noOtherOS && !otherOS.Contains(item))
                {
                    otherOS += item + "|";
                }
            }
            otherOS = otherOS.TrimEnd('|');

            StringBuilder signerBuilder = new StringBuilder();
            foreach (var signer in sigcheckData.Signers)
            {
                signerBuilder.Append(signer.ToString()).Append('-');

                if (signer.Name.Contains(whcp))
                {
                    var vusage = signer.ValidUsages;
                    if (vusage.Exists(x => x.Equals(vuWHQL, StringComparison.OrdinalIgnoreCase)) &&
                        vusage.Exists(x => x.Equals(vuAttestation, StringComparison.OrdinalIgnoreCase))
                        )
                    {
                        attestation = true;
                    }
                    else if (vusage.Exists(x => x.Equals(vuWHQL, StringComparison.OrdinalIgnoreCase)) &&
                        vusage.Exists(x => x.Equals(vuLifetime, StringComparison.OrdinalIgnoreCase))
                        )
                    {
                        testSigning = true;
                        if (string.IsNullOrEmpty(tsExpiryDate))
                        {
                            tsExpiryDate = signer.ValidTo;
                        }
                        else if (DateTime.Parse(tsExpiryDate) > DateTime.Parse(signer.ValidTo))
                        {
                            tsExpiryDate = signer.ValidTo;
                        }
                    }
                    else if (vusage.Exists(x => x.Equals(vuWHQL, StringComparison.OrdinalIgnoreCase)))
                    {
                        whql = true;
                    }
                }
            }

            signerInfo = signerBuilder.ToString().TrimEnd('-');

            summary = getSummary();
        }

        private string getSummary()
        {
            StringBuilder summBuilder = new StringBuilder();
            if (preProd)
            {
                summBuilder.Append("Pre-production signed + ");
            }
            if (testSigning)
            {
                if (!whql)
                {
                    summBuilder.Append("Test-signed + ");
                }
                else
                {
                    summBuilder.Append("Duo-signed (TS + WHQL) + ");
                }
            }
            if (attestation)
            {
                if (!whql)
                {
                    summBuilder.Append("Attestation-signed + ");
                }
                else
                {
                    summBuilder.Append("WHQL signed + ");
                }
            }
            if (whql && !testSigning && !attestation)
            {
                summBuilder.Append("WHQL signed + ");
            }
            if (!whql && !testSigning && !attestation)
            {
                summBuilder.Append("No signed + ");
            }

            summary = summBuilder.ToString().TrimEnd([ ' ', '+' ]);
            return summary;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(name).Append(',').Append(summary).Append(',').Append(filePath).Append(',');
            if (preProd) { sb.Append('O'); }
            sb.Append(',');
            if (attestation) { sb.Append('O'); }
            sb.Append(',');
            if (testSigning) { sb.Append('O'); }
            sb.Append(',');
            if (whql) { sb.Append('O'); }
            sb.Append(',');

            foreach (var item in osversion)
            {
                if (item.Item2)
                {
                    sb.Append('O');
                }
                sb.Append(',');
            }

            sb.Append(otherOS).Append(',');
            sb.Append(tsExpiryDate);
            sb.Append(',').Append(signerInfo);

            return sb.ToString();
        }
    }
}
