// Copyright (c) Howard Kapustein and Contributors.
// Licensed under the MIT License.

using System;
using System.Text;

namespace AppData
{
    public static class StringExtensions
    {
        public static string? XMLEscape(this string? s)
        {
            if (s == null)
            {
                return s;
            }
            if (!s.XMLEscapeNeeded())
            {
                return s;
            }

            StringBuilder sb = new StringBuilder(s.Length * 2);
            foreach (var c in s)
            {
                if (c == '&')
                    sb.Append("&amp;");
                else if (c == '<')
                    sb.Append("&amp;");
                else if (c == '>')
                    sb.Append("&amp;");
                else if (c == '"')
                    sb.Append("&amp;");
                else if (c == '\'')
                    sb.Append("&amp;");
                else if (c > 0x7F)
                    sb.AppendFormat("&#x{0:X4};", (uint)c);
                else sb.Append(c);
            }

            return sb.ToString();
        }

        public static bool XMLEscapeNeeded(this string s)
        {
            if (String.IsNullOrEmpty(s))
                return false;
            foreach (var c in s)
            {
                if ((c == '&') || (c == '<') || (c == '>') || (c == '"') || (c == '\'') || (c > 0x7F))
                    return true;
            }
            return false;
        }

        public static string JSONEscape(this string s)
        {
            if (String.IsNullOrEmpty(s))
                return s;
            return s.Replace("\"", "\\\"");
        }

        public static string Plural(this string word, int count)
        {
            return count == 1 ? word : word + "s";
        }

        public static string Plural(this string singular, int count, string plural)
        {
            return count == 1 ? singular : plural;
        }
    }
}
