using System.Reflection;
using System.Text;

namespace SRHWiscMano.Core.Helpers
{
    public static class StringExtends
    {
        public static string[] SplitBySpace(this string myStr)
        {
            return myStr.Split(new string[] {" "}, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string ToStringJoin<T>(this T[] array, string seperator = ", ", string formatter = null)
        {
            string resStr = "";
            MethodInfo methodInfo = typeof(T).GetMethod("ToString", new[] { typeof(string) });
            if(methodInfo != null)
            {
                var res = array.Select(item => methodInfo.Invoke(item, new object[] {formatter}));
                resStr = string.Join(seperator, res);
            }
            else
            {
                var res = array.Select(item => $"{item}");
                resStr = string.Join(seperator, res);
            }
            return resStr;
        }

        public static string ToStringJoin<T>(this List<T> array, string seperator = ", ", IFormatProvider formatter = null)
        {
            MethodInfo methodInfo = typeof(T).GetMethod("ToString", new[] { typeof(IFormatProvider) });
            string resStr = "";
            if (methodInfo != null)
            {
                var res = array.Select(item => methodInfo.Invoke(item, new object[] { formatter }));
                resStr = string.Join(seperator, res);
            }
            else
            {
                var res = array.Select(item => $"{item}");
                resStr = string.Join(seperator, res);
            }
            return resStr;
        }


        public static string ToStringDictionary<TKey, TVal>(this Dictionary<TKey, TVal> myDic) 
        {
            MethodInfo keyMethodInfo = typeof(TKey).GetMethod("ToString", new Type[] { });
            MethodInfo valMethodInfo = typeof(TVal).GetMethod("ToString", new Type[] { });

            var fun =
                myDic.Select<KeyValuePair<TKey, TVal>, string>((pair, i) => (string)keyMethodInfo.Invoke(pair.Key, null) + "=" + (string)valMethodInfo.Invoke(pair.Value, null));
            string resStr = "{" + string.Join(", ", fun) + "}";

            return resStr;
        }

        public static string Indent(this string value, int size, bool firstIndent = false)
        {
            var strArray = value.Split('\n');
            var sb = new StringBuilder();
            for (int idx = 0; idx < strArray.Length; idx++)
            {
                if (idx == 0 && firstIndent == false)
                {
                    sb.Append(strArray[idx]);
                    continue;
                }
                sb.Append(new string(' ', size)).Append(strArray[idx]);
            }

            return sb.ToString();
        }

    }
}
