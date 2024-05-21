using NodaTime;

namespace SRHWiscMano.Core.Models
{
    /// <summary>
    /// 데이터에서 특정 시간에 대해 Note 정보를 저장하기 위한 클래스
    /// </summary>
    public class FrameNote
    {
        private readonly string bolus;
        public string Text { get; set; }
        public string Bolus { get; set; }
        public string Unit { get; set; } = "cc";
        public Instant Time { get; set; }

        public FrameNote(Instant time, string text)
        {
            this.Time = time;
            this.Text = text;
        }

        public FrameNote(Instant time, string bolus, string text)
        {
            this.Time = time;
            this.bolus = bolus;
            this.Text = text;
        }

        public override string ToString()
        {
            return $"{bolus}{Unit} {Text}";
        }
    }
}