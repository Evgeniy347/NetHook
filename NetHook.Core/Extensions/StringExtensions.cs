using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetHook.Cores.Extensions
{
    public static class StringExtensions
    {
        public static bool ContainsSearch(this string source, string rawSearch)
        {
            string rusKey = "Ё!\"№;%:?*()_+ЙЦУКЕНГШЩЗХЪ/ФЫВАПРОЛДЖЭЯЧСМИТЬБЮ,ё1234567890-=йцукенгшщзхъ\\фывапролджэячсмитьбю. ";
            string engKey = "~!@#$%^&*()_+QWERTYUIOP{}|ASDFGHJKL:\"ZXCVBNM<>?`1234567890-=qwertyuiop[]\\asdfghjkl;'zxcvbnm,./ ";

            rawSearch = rawSearch.ToLower();

            string rusSearch = rawSearch;
            string engSearch = rawSearch;

            for (int i = 0; i < rusKey.Length; i++)
            {
                rusSearch = rusSearch?.Replace(engKey[i], rusKey[i]);
                engSearch = engSearch?.Replace(rusKey[i], engKey[i]);
            }

            return source.ContainsString(rawSearch, rusSearch, engSearch);
        }


        public static bool ContainsString(this string source, params string[] finds)
        {
            source = source.ToLower();

            foreach (string find in finds)
            {
                if (source.Contains(find?.ToLower()))
                    return true;
            }

            return false;
        }

        public static string JoinString<TSourse>(this IEnumerable<TSourse> sourses, string separator = ",") =>
            sourses.Select(x => x.ToString()).JoinString(separator);

        public static string JoinString(this IEnumerable<string> sourses, string separator = ",") =>
             sourses.ToArray().JoinString(separator);

        public static string JoinString(this string[] sourses, string separator = ",") =>
            string.Join(separator, sourses);

    }
}
