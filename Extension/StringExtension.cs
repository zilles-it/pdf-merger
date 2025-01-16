/*
    This file is part of the pdf-merger project.
    Copyright (c) 2025 ZILLES-IT GmbH

    AGPL licensing:
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

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
