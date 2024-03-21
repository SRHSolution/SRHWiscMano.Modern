namespace SRHWiscMano.Core.Helpers
{
    public static class RangeExtensions
    {
        public static Decimal Span(this Range<Decimal> range) => range.End - range.Start;

        public static double Span(this Range<double> range) => range.End - range.Start;

        public static float Span(this Range<float> range) => range.End - range.Start;

        public static int Span(this Range<int> range) => range.End - range.Start;

        public static long Span(this Range<long> range) => range.End - range.Start;

        public static TimeSpan Span(this Range<DateTime> range) => range.End - range.Start;

        public static double PercentOf(this Range<Decimal> range, Decimal value) => (double)(value - range.Start) / (double)range.Span();

        public static double PercentOf(this Range<double> range, double value) => (value - range.Start) / range.Span();

        public static double PercentOf(this Range<float> range, float value) => ((double)value - (double)range.Start) / (double)range.Span();

        public static double PercentOf(this Range<int> range, int value) => (double)(value - range.Start) / (double)range.Span();

        public static double PercentOf(this Range<long> range, long value) => (double)(value - range.Start) / (double)range.Span();

        public static IEnumerable<int> AsEnumerable(this Range<int> range) => Enumerable.Range(range.Start, range.Span());

        public static int ValueAt(this Range<int> range, double percent)
        {
            return range.Start + (int)(range.Span() * percent);
        }

        public static double ValueAt(this Range<double> range, double percent)
        {
            return range.Start + range.Span() * percent;
        }

        public static int TranslateFrom(
            this Range<int> targetDomain,
            Range<int> sourceDomain,
            int valueInSourceDomain)
        {
            return targetDomain.ValueAt(sourceDomain.PercentOf(valueInSourceDomain));
        }

        public static double TranslateFrom(
            this Range<double> targetDomain,
            Range<double> sourceDomain,
            double valueInSourceDomain)
        {
            return targetDomain.ValueAt(sourceDomain.PercentOf(valueInSourceDomain));
        }

        public static Range<double> CompressBy(this Range<double> range, double percent)
        {
            double num = 1.0 - percent;
            return Range.Create(range.Start * num, range.End * num);
        }

        public static double Center(this Range<double> range)
        {
            return range.Start + range.Span() / 2.0;
        }

        public static Range<int> SetSpan(this Range<int> range, int newSpan)
        {
            return Range.Create(range.Start, range.Start + newSpan);
        }
    }
}
