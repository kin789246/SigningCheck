using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SigningCheck
{
    public static class HtmlHelper
    {
        private const string tagBody = @"<body>";
        private const string endTagBody = @"</body>";
        private const string tagH1 = @"<h1>";
        private const string endTagH1 = @"</h1>";
        private const string tagTable = @"<table>";
        private const string endTagTable = @"</table>";
        private const string tagTh = @"<th>";
        private const string endTagTh = @"</th>";
        private const string tagTr = @"<tr>";
        private const string endTagTr = @"</tr>";
        private const string tagTd = @"<td>";
        private const string endTagTd = @"</td>";
        private const string tagBr = @"<br />";
        private const string tagStyle = @"<style>";
        private const string tagScript = @"<script>";

        private const string styleName = "config\\style.css";
        private const string scriptName = "config\\main.js";

        private static void addStringToTd(StringBuilder sb, string str)
        {
            sb.Append(tagTd);
            sb.Append(str);
            sb.Append(endTagTd);
        }

        private static void addOToTd(StringBuilder sb, bool support)
        {
            if (support) { addStringToTd(sb, "O"); }
            else { addStringToTd(sb, ""); }
        }
        private static string makeTable(CsvOutData data)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(tagH1).Append(data.Summary).Append(endTagH1);

            //table part
            sb.Append(tagTable);
            sb.Append(tagTr);
            //NO.,Name,Summary,Path,PreProd,Attestation,TestSigning,WHQL,24H2,22H2,21H2,OtherOS,Expiry,Signers{FileName||ValidUsages||SigningDate||ValidFrom||ValidTo}
            sb.Append(tagTh).Append("NO.").Append(endTagTh);
            foreach (var item in data.Title.Split(','))
            {
                sb.Append(tagTh).Append(item).Append(endTagTh);
            }
            sb.Append(endTagTr);
            int i = 1;
            string rgxVbar = @"\w+\|\w";
            foreach (var csvData in data.Data)
            {
                sb.Append(tagTr);
                addStringToTd(sb, i.ToString()); i++;
                addStringToTd(sb, csvData.Name);
                addStringToTd(sb, csvData.Summary);
                addStringToTd(sb, csvData.FilePath);
                addOToTd(sb, csvData.PreProd);
                addOToTd(sb, csvData.Attestation);
                addOToTd(sb, csvData.TestSigning);
                addOToTd(sb, csvData.Whql);
                foreach (var os in csvData.Osversion)
                {
                    addOToTd(sb, os.Item2);
                }
                if (Regex.IsMatch(csvData.OtherOS, rgxVbar))
                {
                    string str = string.Empty;
                    foreach (var os in csvData.OtherOS.Split('|'))
                    {
                        str += os + tagBr;
                    }
                    addStringToTd(sb, str);
                }
                else
                {
                    addStringToTd(sb, csvData.OtherOS);
                }
                addStringToTd(sb, csvData.TsExpiryDate);
                string preStr = string.Empty;
                foreach (var signer in csvData.SigcheckData.Signers)
                {
                    //preStr += signer + tagBr;
                    preStr += signer.Name + " {" + tagBr;
                    foreach (var vu in signer.ValidUsages)
                    {
                        preStr += vu + ", ";
                    }
                    preStr += tagBr +
                        "date:" + signer.SigningDate + "||from:" + signer.ValidFrom + "||to:" + signer.ValidTo + tagBr +
                        "}" + tagBr;
                }
                addStringToTd(sb, preStr);
                sb.Append(endTagTr);
            }
            sb.Append(endTagTable);
            return sb.ToString();

        }
        private static string readFrom(string name)
        {
            StringBuilder sb = new StringBuilder();
            using (StreamReader sr = new StreamReader(name))
            {
                sb.Append(sr.ReadToEnd());
            }
            return sb.ToString();
        }
        public static string ToHtmlTable(CsvOutData data)
        {
            StringBuilder sb = new StringBuilder();
            string reportT = "config\\report";
            using (StreamReader sr = new StreamReader(reportT))
            {
                try
                {
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        sb.Append(line);
                        if (line.Trim().Equals(tagStyle, StringComparison.OrdinalIgnoreCase))
                        {
                            sb.Append(readFrom(styleName));
                        }
                        if (line.Trim().Equals(tagBody, StringComparison.OrdinalIgnoreCase))
                        {
                            sb.Append(makeTable(data));
                        }
                        if (line.Trim().Equals(tagScript, StringComparison.OrdinalIgnoreCase))
                        {
                            sb.Append(readFrom(scriptName));
                        }
                        
                        line = sr.ReadLine();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            return sb.ToString();
        }
    }
}

/*
    <!DOCTYPE html>
    <html>
    <head>
    <style>
    table, th, td {
        border-style: solid;
        border-width: 1px;
        border-collapse: collapse;
    }
    </style>
    </head>
    <body>

    <h1>summary line</h1>

    <table>
      <tr>
        <th>name</th>
        <th>summary</th>
        <th>path</th>
      </tr>
      <tr>
        <td>acxdac.cat</td>
        <td>WHQL signed</td>
        <td>\INT_VGA_MTL_101.5522_WHQL_22H2\Driver\dchu_5522</td>
      </tr>
      <tr>
        <td>acxdac.cat</td>
        <td>WHQL signed</td>
        <td>\INT_VGA_MTL_101.5522_WHQL_22H2\Driver\dchu_5522</td>
      </tr>
    </table>

    </body>
    </html>
 */
