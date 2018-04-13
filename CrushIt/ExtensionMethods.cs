using System;
using System.Globalization;

namespace CrushIt
{
    public static class CrushIt
    {
        public static CultureInfo Culture = new CultureInfo("en-GB");
    }

    public static class ExtensionMethods
    { 
        public static int ToInt(this string s)
        {
            return int.Parse(s);
        }

        public static float ToSingle(this string s)
        {
            return float.Parse(s, CrushIt.Culture);
        }

        public static T ToEnum<T>(this string value, bool ignoreCase = true)
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase);
        }

        public static T ToEnumWithDefault<T>(this string value, T defaultValue, bool ignoreCase = true) where T : struct
        {
            if (Enum.TryParse<T>(value, ignoreCase, out T o))
            {
                return o;
            }
            else
            {
                return defaultValue;
            }
        }

        public static string Replace(this string originalString, string oldValue, string newValue, StringComparison comparisonType)
        {
            int startIndex = 0;
            while (true)
            {
                startIndex = originalString.IndexOf(oldValue, startIndex, comparisonType);
                if (startIndex == -1) { break; }

                originalString = originalString.Substring(0, startIndex) + newValue + originalString.Substring(startIndex + oldValue.Length);

                startIndex += newValue.Length;
            }

            return originalString;
        }
    }
}
