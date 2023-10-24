namespace SRHWiscMano.Core.Helpers
{
    internal class FuncComparer<T> : IComparer<T>
    {
        private readonly Func<T, T, int> comparer;

        public FuncComparer(Func<T, T, int> comparer) => this.comparer = comparer;

        public int Compare(T x, T y) => this.comparer(x, y);
    }
}
