using System.Globalization;

namespace SRHWiscMano.Core.Helpers
{
    public static class StringConversionExtensions
    {
        public static string EmptyIfNull(this string s) => s != null ? s : string.Empty;

        public static bool? AsBool(this string s)
        {
            bool result;
            return !string.IsNullOrEmpty(s) && bool.TryParse(s, out result) ? new bool?(result) : new bool?();
        }

        public static bool AsBoolOrDefault(this string s) => s.AsBoolOrDefault(false);

        public static bool AsBoolOrDefault(this string s, bool defaultValue)
        {
            bool? nullable = s.AsBool();
            return !nullable.HasValue ? defaultValue : nullable.Value;
        }

        public static int? AsInt(this string s)
        {
            int result;
            return !string.IsNullOrEmpty(s) && int.TryParse(s, out result) ? new int?(result) : new int?();
        }

        public static int AsIntOrDefault(this string s) => s.AsIntOrDefault(0);

        public static int AsIntOrDefault(this string s, int defaultValue)
        {
            int? nullable = s.AsInt();
            return !nullable.HasValue ? defaultValue : nullable.Value;
        }

        public static long? AsLong(this string s)
        {
            long result;
            return !string.IsNullOrEmpty(s) && long.TryParse(s, out result) ? new long?(result) : new long?();
        }

        public static long AsLongOrDefault(this string s) => s.AsLongOrDefault(0L);

        public static long AsLongOrDefault(this string s, long defaultValue)
        {
            long? nullable = s.AsLong();
            return !nullable.HasValue ? defaultValue : nullable.Value;
        }

        public static double? AsDouble(this string s)
        {
            double result;
            return !string.IsNullOrEmpty(s) && double.TryParse(s, out result) ? new double?(result) : new double?();
        }

        public static double AsDoubleOrDefault(this string s) => s.AsDoubleOrDefault(0.0);

        public static double AsDoubleOrDefault(this string s, double defaultValue)
        {
            double? nullable = s.AsDouble();
            return !nullable.HasValue ? defaultValue : nullable.Value;
        }

        public static Decimal? AsDecimal(this string s)
        {
            Decimal result;
            return !string.IsNullOrEmpty(s) && Decimal.TryParse(s, out result) ? new Decimal?(result) : new Decimal?();
        }

        public static Decimal AsDecimalOrDefault(this string s) => s.AsDecimalOrDefault(0M);

        public static Decimal AsDecimalOrDefault(this string s, Decimal defaultValue)
        {
            Decimal? nullable = s.AsDecimal();
            return !nullable.HasValue ? defaultValue : nullable.Value;
        }

        public static DateTime? AsDateTime(this string s)
        {
            DateTime result;
            return !string.IsNullOrEmpty(s) && DateTime.TryParse(s, out result) ? new DateTime?(result) : new DateTime?();
        }

        public static DateTime? AsDateTimeRoundtripKind(this string s)
        {
            DateTime result;
            return !string.IsNullOrEmpty(s) && DateTime.TryParse(s, (IFormatProvider)null, DateTimeStyles.RoundtripKind, out result) ? new DateTime?(result) : new DateTime?();
        }

        public static DateTime AsDateTimeOrDefault(this string s) => s.AsDateTimeOrDefault(new DateTime());

        public static DateTime AsDateTimeOrDefault(this string s, DateTime defaultValue)
        {
            DateTime? nullable = s.AsDateTime();
            return !nullable.HasValue ? defaultValue : nullable.Value;
        }
    }
}
