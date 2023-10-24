namespace SRHWiscMano.Core.Helpers
{
    public static class Compare
    {
        public static IEqualityComparer<T> With<T>(Func<T, T, bool> equality) => (IEqualityComparer<T>)new FuncEqualityComparer<T>(equality);

        public static IEqualityComparer<T> With<T>(Func<T, T, bool> equality, Func<T, int> hashCode) => (IEqualityComparer<T>)new FuncEqualityComparer<T>(equality, hashCode);

        public static IComparer<T> With<T>(Func<T, T, int> comparer) => (IComparer<T>)new FuncComparer<T>(comparer);

        public static IComparer<T> WithLessThan<T>(Func<T, T, bool> lessThan) => (IComparer<T>)new FuncComparer<T>((Func<T, T, int>)((a, b) =>
        {
            if (lessThan(a, b))
            {
                if (lessThan(b, a))
                    throw new InvalidOperationException();
                return -1;
            }
            return !lessThan(b, a) ? 0 : 1;
        }));

        public static IEqualityComparer<double> Doubles(int precision)
        {
            double delta = precision >= 0 ? Math.Pow(10.0, (double)-precision) : throw new ArgumentOutOfRangeException(nameof(precision), (object)precision, "Precision must be zero or positive");
            return Compare.With<double>((Func<double, double, bool>)((a, b) => Math.Abs(b - a) < delta));
        }
    }
}
