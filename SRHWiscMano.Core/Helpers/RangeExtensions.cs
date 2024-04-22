namespace SRHWiscMano.Core.Helpers
{
    public static class RangeExtensions
    {
        public static Decimal Span(this Range<Decimal> range) => range.Greater - range.Lesser;

        public static double Span(this Range<double> range) => range.Greater - range.Lesser;

        public static float Span(this Range<float> range) => range.Greater - range.Lesser;

        public static int Span(this Range<int> range) => range.Greater - range.Lesser;

        public static long Span(this Range<long> range) => range.Greater - range.Lesser;

        public static TimeSpan Span(this Range<DateTime> range) => range.Greater - range.Lesser;

        public static double PercentOf(this Range<Decimal> range, Decimal value) => (double)(value - range.Lesser) / (double)range.Span();

        public static double PercentOf(this Range<double> range, double value) => (value - range.Lesser) / range.Span();

        public static double PercentOf(this Range<float> range, float value) => ((double)value - (double)range.Lesser) / (double)range.Span();

        public static double PercentOf(this Range<int> range, int value) => (double)(value - range.Lesser) / (double)range.Span();

        public static double PercentOf(this Range<long> range, long value) => (double)(value - range.Lesser) / (double)range.Span();

        public static IEnumerable<int> AsEnumerable(this Range<int> range) => Enumerable.Range(range.Lesser, range.Span()+1);

        public static int ValueAt(this Range<int> range, double percent)
        {
            return range.Lesser + (int)(range.Span() * percent);
        }

        public static double ValueAt(this Range<double> range, double percent)
        {
            return range.Lesser + range.Span() * percent;
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
            return Range.Create(range.Lesser * num, range.End * num);
        }

        public static double Center(this Range<double> range)
        {
            return range.Lesser + range.Span() / 2.0;
        }

        public static Range<int> SetSpan(this Range<int> range, int newSpan)
        {
            if(range.IsForward)
                return Range.Create(range.Start, range.Start + newSpan);
            else
            {
                return Range.Create(range.Start, range.Start - newSpan);
            }
        }
    }
}
