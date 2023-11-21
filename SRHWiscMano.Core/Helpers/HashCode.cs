namespace SRHWiscMano.Core.Helpers
{
    public struct HashCode
    {
        private const int Factor = 31;
        private readonly int hashCode;

        public HashCode(int hashCode) => this.hashCode = hashCode;

        public static implicit operator int(HashCode hashCode) => hashCode.GetHashCode();

        public static HashCode Compose<T>(T arg) => new(arg.GetHashCode());

        public static HashCode ComposeNotNull<T>(T arg) where T : class => new((object)arg == null ? 31 : arg.GetHashCode());

        public HashCode And<T>(T arg) => new(this.hashCode * 31 + arg.GetHashCode());

        public HashCode AndNotNull<T>(T arg) => new(this.hashCode * 31 + ((object)arg == null ? 0 : arg.GetHashCode()));

        public override int GetHashCode() => this.hashCode;
    }
}
