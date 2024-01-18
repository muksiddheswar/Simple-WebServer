using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bare.Extensions
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Left of the first occurance of c
        /// </summary>
        /// <param name="src">The source string.</param>
        /// <param name="c">Return everything to the left of this character.</param>
        /// <returns>String to the left of c, or the entire string.</returns>
        public static string LeftOfChar(string src, char c)
        {
            string ret = src;

            int index = src.IndexOf(c);

            if (index != -1)
            {
                ret = src[..index];
            }

            return ret;
        }

        /// <summary>
        /// Left of the n'th occurance of c.
        /// </summary>
        /// <param name="src">The source string.</param>
        /// <param name="c">Return everything to the left n'th occurance of this character.</param>
        /// <param name="n">The occurance.</param>
        /// <returns>String to the left of c, or the entire string if not found or n is 0.</returns>
        public static string LeftOfChar(string src, char c, int n)
        {
            string ret = src;
            int index = -1;

            while (n > 0)
            {
                index = src.IndexOf(c, index + 1);

                if (index == -1)
                {
                    break;
                }

                --n;
            }

            if (index != -1)
            {
                ret = src.Substring(0, index);
            }

            return ret;
        }

        /// <summary>
        /// Right of the first occurance of c
        /// </summary>
        /// <param name="src">The source string.</param>
        /// <param name="c">The search char.</param>
        /// <returns>Returns everything to the right of c, or an empty string if c is not found.</returns>
        public static string RightOfChar(string src, char c)
        {
            string ret = String.Empty;
            int index = src.IndexOf(c);

            if (index != -1)
            {
                ret = src[(index + 1)..];
            }

            return ret;
        }

        /// <summary>
        /// Returns all the text to the right of the specified string.
        /// Returns an empty string if the substring is not found.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="substr"></param>
        /// <returns></returns>
        public static string RightOfChar(string src, string substr)
        {
            string ret = String.Empty;
            int index = src.IndexOf(substr);

            if (index != -1)
            {
                ret = src[(index + substr.Length)..];
            }

            return ret;
        }
    }
}
