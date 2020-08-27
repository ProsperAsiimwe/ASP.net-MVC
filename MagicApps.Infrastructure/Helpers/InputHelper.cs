using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MagicApps.Infrastructure.Helpers
{
    public class InputHelper
    {
        public static string StringParam(string inputValue)
        {
            if (string.IsNullOrEmpty(inputValue))
            {
                return string.Empty;
            }

            inputValue = inputValue.Trim();
            inputValue = inputValue.Replace(System.Environment.NewLine, " ");
            inputValue = inputValue.Replace(@"""", "'");

            return String.Format(@"""{0}""", inputValue);
        }
        
        public static string PCase(string inputValue)
        {
            string strReturn; string[] words;

            words = inputValue.Trim().Split(' ');
            strReturn = "";

            foreach (string word in words) {
                if (strReturn != "") {
                    strReturn += " ";
                }
                if (word.Length > 0) {
                    strReturn += word.Substring(0, 1).ToUpper();
                    strReturn += word.Substring(1, (word.Length - 1)).ToLower();
                }
            }

            return strReturn.Trim();
        }

        public static string ParseMoney(double? amount)
        {
            if (!amount.HasValue)
            {
                return "-";
            }

            return amount > 0 ? string.Format("{0} Ugx", amount.Value.ToString("n0")) : "-";
        }

        public static string ParseBool(bool? which)
        {
            if (which.HasValue) {
                if (which.Value) {
                    return "Yes";
                }
                else {
                    return "No";
                }
            }
            else {
                return "-";
            }
        }

        public static string ParseDate(DateTime? date, string format = "ddd, dd MMM yyyy")
        {
            if (date.HasValue) {
                return date.Value.ToString(format);
            }
            else {
                return "-";
            }
        }

        public static string ParseString(string str, string @default = "-")
        {
            if (!String.IsNullOrEmpty(str)) {
                return str;
            }
            else {
                return @default;
            }
        }

        public static string ParseInt(int @int, string @default = "-")
        {
            if (@int > 0) {
                return @int.ToString();
            }
            else {
                return @default;
            }
        }
    }
}