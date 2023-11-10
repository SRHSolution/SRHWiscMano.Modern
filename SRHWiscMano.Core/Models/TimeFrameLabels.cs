namespace SRHWiscMano.Core.Models
{
    public class TimeFrameLabels : ITimeFrameLabels
    {
        private readonly string strategy;
        private readonly string texture;
        private readonly string volume;

        public TimeFrameLabels(string volume, string texture, string strategy)
        {
            this.volume = volume;
            this.texture = texture;
            this.strategy = strategy;
        }

        public string Volume => volume;

        public string Texture => texture;

        public string Strategy => strategy;

        public static TimeFrameLabels Empty()
        {
            return new TimeFrameLabels(null, null, null);
        }
    }
}