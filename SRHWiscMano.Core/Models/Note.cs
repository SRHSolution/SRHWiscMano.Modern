using NodaTime;

namespace SRHWiscMano.Core.Models
{
    public class Note
    {
        private readonly string text;
        private Instant time;

        public Note(Instant time, string text)
        {
            this.time = time;
            this.text = text;
        }

        public Instant Time
        {
            get => time;
            set => time = value;
        }

        public string Text => text;
    }
}