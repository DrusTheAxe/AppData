using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace AppData
{
    /// <summary>
    /// Represents a wildcard running on the
    /// <see cref="System.Text.RegularExpressions"/> engine.
    /// </summary>
    public class Wildcard : Regex
    {
        [Flags]
        public enum WildcardOptions
        {
            /// <summary>
            /// Specifies that cultural differences in language is ignored. 
            /// See Performing Culture-Insensitive Operations in the RegularExpressions namespace for more information.
            /// </summary>
            CultureInvariant = RegexOptions.CultureInvariant,

            /// <summary>
            /// Specifies case-insensitive matching.
            /// </summary>
            IgnoreCase = RegexOptions.IgnoreCase,

            /// <summary>
            /// Specifies that no options are set.
            /// </summary>
            None = RegexOptions.None
        }

        /// <summary>
        /// Initializes a wildcard with the given search pattern.
        /// </summary>
        /// <param name="pattern">The wildcard pattern to match.</param>
        public Wildcard(string pattern)
            : base(WildcardToRegex(pattern), RegexOptions.Singleline)
        {
        }

        /// <summary>
        /// Initializes a wildcard with the given search pattern and options.
        /// </summary>
        /// <param name="pattern">The wildcard pattern to match.</param>
        /// <param name="options">A combination of one or more
        /// <see cref="WildCardOptions"/>.</param>
        public Wildcard(string pattern, WildcardOptions options)
            : base(WildcardToRegex(pattern), (RegexOptions)((int) options | (int) RegexOptions.Singleline))
        {
        }

        /// <summary>
        /// Converts a wildcard to a regex.
        /// </summary>
        /// <param name="pattern">The wildcard pattern to convert.</param>
        /// <returns>A regex equivalent of the given wildcard.</returns>
        public static string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern).
             Replace("\\*", ".*").
             Replace("\\?", ".") + "$";
        }
    }
}
