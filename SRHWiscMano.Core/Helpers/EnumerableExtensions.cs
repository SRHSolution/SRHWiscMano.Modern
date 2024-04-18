﻿namespace SRHWiscMano.Core.Helpers
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

        public static IEnumerable<Tuple<T, int>> SelectWithIndex<T>(
            this IEnumerable<T> source)
        {
            return source.Select((a, i) => Tuple.Create(a, i));
        }

        public static int? FirstIndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            int num = 0;
            foreach (T obj in source)
            {
                if (predicate(obj))
                    return num;
                ++num;
            }

            return new int?();
        }

        public static int? LastIndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            int num = 0;
            int? nullable = new int?();
            foreach (T obj in source)
            {
                if (predicate(obj))
                    nullable = num;
                ++num;
            }

            return nullable;
        }

        public static List<T2> Map<T1, T2>(this IReadOnlyCollection<T1> list, Func<T1, T2> selector)
        {
            List<T2> objList = new List<T2>(list.Count);
            foreach (T1 obj in list)
                objList.Add(selector(obj));
            return objList;
        }
    }
}
