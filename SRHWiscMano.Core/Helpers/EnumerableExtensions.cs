namespace SRHWiscMano.Core.Helpers
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Cycle<T>(this IEnumerable<T> sequence)
        {
            label_1:
            using (IEnumerator<T> enumerator = sequence.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    T item = enumerator.Current;
                    yield return item;
                }
                goto label_1;
            }
        }

        public static IEnumerable<Tuple<T, T>> Pairwise<T>(this IEnumerable<T> sequence)
        {
            T previous = default(T);
            using (IEnumerator<T> it = sequence.GetEnumerator())
            {
                if (it.MoveNext())
                {
                    previous = it.Current;
                    while (it.MoveNext())
                    {
                        yield return Tuple.Create<T, T>(previous, it.Current);
                        previous = it.Current;
                    }
                }
            }
        }

        public static IEnumerable<T> Separator<T>(this IEnumerable<T> sequence, T separator)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));
            using (IEnumerator<T> it = sequence.GetEnumerator())
            {
                if (it.MoveNext())
                {
                    yield return it.Current;
                    while (it.MoveNext())
                    {
                        yield return separator;
                        yield return it.Current;
                    }
                }
            }
        }
    }
}
