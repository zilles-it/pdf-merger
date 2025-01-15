using System.Globalization;
using System;

namespace pdf_merger.Extension
{
    internal static class StringExtension
    {
        public static bool IsBlank(this string text)
        {
            return string.IsNullOrWhiteSpace(text);
        }

        public static bool IsNotBlank(this string text)
        {
            return !text.IsBlank();
        }

        public static string TrimToEmpty(this string s)
        {
            return (s ?? "").Trim();
        }

        public static string Left(this string str, int count)
        {
            if (count <= 0)
            {
                return "";
            }

            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            if (str.Length > count)
            {
                return str.Substring(0, count);
            }
            else
            {
                return str;
            }
        }

        public static string EscapeXml(this object obj)
        {
            if (obj == null)
            {
                return "";
            }

            string strValue = "";

            if (obj is int)
            {
                strValue = obj.ToString();
            }
            else if (obj is DateTime dateTime)
            {
                strValue = dateTime.ToUniversalTime().ToString("s") + "Z";
            }
            else if (obj is bool)
            {
                strValue = obj.ToString().ToLower();
            }
            else if (obj is decimal d)
            {
                strValue = d.ToString("G", CultureInfo.InvariantCulture);
            }
            else if (obj is string s)
            {
                strValue = s;
            }

            strValue = strValue.Replace("&", "&amp;")
                .Replace("'", "&apos;")
                .Replace("\"", "&quot;")
                .Replace(">", "&gt;")
                .Replace("<", "&lt;");

            return strValue;
        }
    }
}
