namespace SRHWiscMano.Core.Models
{
    public class TimeFrameLabels : ITimeFrameLabels
    {
        public static TimeFrameLabels Empty = new(string.Empty, string.Empty, string.Empty);
        public string Volume { get; }

        public string Texture { get; }

        public string Strategy { get; }

        public TimeFrameLabels(string volume, string texture, string strategy)
        {
            this.Volume = volume;
            this.Texture = texture;
            this.Strategy = strategy;
        }
    }
}