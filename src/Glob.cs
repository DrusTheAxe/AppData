// Copyright (c) Howard Kapustein and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;

namespace AppData
{
    internal static class Glob
    {
        public static bool Match(
            ReadOnlySpan<char> pattern,
            ReadOnlySpan<char> input,
            StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            int p = 0;
            int s = 0;
            int star = -1;
            int match = 0;

            while (s < input.Length)
            {
                if (p < pattern.Length &&
                    (pattern[p] == '?' ||
                     pattern[p] == '*' ||
                     EqualsOrdinal(pattern[p], input[s], comparison)))
                {
                    if (pattern[p] == '*')
                    {
                        star = p++;
                        match = s;
                    }
                    else
                    {
                        p++;
                        s++;
                    }
                }
                else if (star != -1)
                {
                    p = star + 1;
                    s = ++match;
                }
                else
                {
                    return false;
                }
            }

            // Skip trailing *
            while (p < pattern.Length && pattern[p] == '*')
            {
                p++;
            }

            return p == pattern.Length;
        }

        private static bool EqualsOrdinal(
            char a,
            char b,
            StringComparison comparison)
        {
            if (comparison == StringComparison.Ordinal)
                return a == b;

            if (comparison == StringComparison.OrdinalIgnoreCase)
                return char.ToUpperInvariant(a) == char.ToUpperInvariant(b);

            throw new ArgumentException(
                "Only Ordinal and OrdinalIgnoreCase are supported",
                nameof(comparison));
        }
    }
}
