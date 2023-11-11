using NodaTime;

namespace SRHWiscMano.Core.Models
{
    /// <summary>
    /// 데이터에서 특정 시간에 대해 Note 정보를 저장하기 위한 클래스
    /// </summary>
    public class FrameNote
    {
        public string Text { get; set; }
        
        public Instant Time { get; set; }

        public FrameNote(Instant time, string text)
        {
            this.Time = time;
            this.Text = text;
        }
    }
}