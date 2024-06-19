using NodaTime;

namespace SRHWiscMano.Core.Models
{
    /// <summary>
    /// 데이터에서 특정 시간에 대해 Note 정보를 저장하기 위한 클래스
    /// </summary>
    public class FrameNote
    {
        public string Text { get; set; }
        public string Bolus { get; set; }
        public string Unit { get; set; } = "cc";
        public Instant Time { get; set; }

        public int version = 1;

        public FrameNote(Instant time, string text)
        {
            this.Time = time;
            this.Text = text;
            if (text.Contains("cc"))
            {
                var divs = text.Split("cc");
                this.Bolus = divs[0].Trim();
                if(divs.Length >= 2)
                    this.Text = divs[1].Trim();
            }
            version = 1;
        }

        public FrameNote(Instant time, string bolus, string text)
        {
            this.Time = time;
            this.Bolus = bolus;
            this.Text = text;
            version = 2;
        }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Bolus))
            {
                return $"{Bolus}{Unit}-{Text}";
            }
            else
            {
                return $"{Text}";
            }
        }
    }
}
