namespace SRHWiscMano.Core.Helpers
{
    public static class GeneralExtensions
    {
        public static R IfNotNull<T, R>(this T obj, Func<T, R> fn) where T : class => (object)obj != null ? fn(obj) : default(R);

        public static R IfNotNull<T, R>(this T obj, Func<T, R> fn, R defaultValue) where T : class => (object)obj != null ? fn(obj) : defaultValue;

        public static R IfHasValue<T, R>(this T? obj, Func<T, R> fn) where T : struct => obj.HasValue ? fn(obj.Value) : default(R);

        public static R IfHasValue<T, R>(this T? obj, Func<T, R> fn, R defaultValue) where T : struct => obj.HasValue ? fn(obj.Value) : defaultValue;

        public static T ValueOrDefault<T>(this T? obj) where T : struct => !obj.HasValue ? default(T) : obj.Value;

        public static T ValueOrDefault<T>(this T? obj, T defaultValue) where T : struct => !obj.HasValue ? defaultValue : obj.Value;
    }
}
