namespace SRHWiscMano.Core.Models
{
    public class SnapshotLabels : ISnapshotLabels
    {
        private readonly string strategy;
        private readonly string texture;
        private readonly string volume;

        public SnapshotLabels(string volume, string texture, string strategy)
        {
            this.volume = volume;
            this.texture = texture;
            this.strategy = strategy;
        }

        public string Volume => volume;

        public string Texture => texture;

        public string Strategy => strategy;

        public static SnapshotLabels Empty()
        {
            return new SnapshotLabels(null, null, null);
        }
    }
}