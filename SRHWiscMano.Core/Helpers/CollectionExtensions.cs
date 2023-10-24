namespace SRHWiscMano.Core.Helpers
{
    public static class CollectionExtensions
    {
        public static ICollection<T> AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (T obj in items)
                collection.Add(obj);
            return collection;
        }
    }
}
