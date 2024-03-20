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
        public static string LeftOfChar(this string src, char c)
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
        public static string LeftOfChar(this string src, char c, int n)
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
        public static string RightOfChar(this string src, char c)
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
        public static string RightOfChar(this string src, string substr)
        {
            string ret = String.Empty;
            int index = src.IndexOf(substr);

            if (index != -1)
            {
                ret = src[(index + substr.Length)..];
            }

            return ret;
        }


        /// <summary>
        /// Returns everything to the left of the righmost char c.
        /// </summary>
        /// <param name="src">The source string.</param>
        /// <param name="c">The search char.</param>
        /// <returns>Everything to the left of the rightmost char c, or the entire string.</returns>
        public static string LeftOfRightmostOf(this string src, char c)
        {
            string ret = src;
            int idx = src.LastIndexOf(c);

            if (idx != -1)
            {
                ret = src.Substring(0, idx);
            }

            return ret;
        }

        /// <summary>
        /// Returns everything to the right of the rightmost char c.
        /// </summary>
        /// <param name="src">The source string.</param>
        /// <param name="c">The seach char.</param>
        /// <returns>Returns everything to the right of the rightmost search char, or an empty string.</returns>
        public static string RightOfRightmostOf(this string src, char c)
        {
            string ret = String.Empty;
            int idx = src.LastIndexOf(c);

            if (idx != -1)
            {
                ret = src.Substring(idx + 1);
            }

            return ret;
        }



        /// <summary>
		/// Returns a new string surrounded by single quotes.
		/// </summary>
		public static string SingleQuote(this String src)
        {
            return "'" + src + "'";
        }




        /// <summary>
		/// Returns true if the object is null.
		/// </summary>
        public static bool IfNull<T>(this T obj)
        {
            return obj == null;
        }

        /// <summary>
        /// If the object is null, performs the action and returns true.
        /// </summary>
        public static bool IfNull<T>(this T obj, Action action)
        {
            bool ret = obj == null;

            if (ret) { action(); }

            return ret;
        }

        /// <summary>
        /// Returns true if the object is not null.
        /// </summary>
        public static bool IfNotNull<T>(this T obj)
        {
            return obj != null;
        }

        /// <summary>
        /// If the object is not null, performs the action and returns true.
        /// </summary>
        public static bool IfNotNull<T>(this T obj, Action<T> action)
        {
            bool ret = obj != null;

            if (ret) { action(obj); }

            return ret;
        }


        /// <summary>
        /// Return the result of the func if 'T is not null, passing 'T to func.
        /// </summary>
        public static R IfNotNullReturn<T, R>(this T obj, Func<T, R> func)
        {
            if (obj != null)
            {
                return func(obj);
            }
            else
            {
                return default(R);
            }
        }

        public static bool If<T>(this T v, Func<T, bool> predicate, Action<T> action)
        {
            bool ret = predicate(v);

            if (ret)
            {
                action(v);
            }

            return ret;
        }




        /// <summary>
        /// Implements a ForEach for generic enumerators.
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);


            }
        }
    }
}
