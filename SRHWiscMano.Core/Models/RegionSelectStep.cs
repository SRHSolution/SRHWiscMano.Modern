using System.Windows.Media;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.Core.Models
{
    public class RegionSelectStep 
    {
        private readonly Color color;
        private readonly TimeFrameViewModel owner;
        private readonly string title;
        private readonly RegionType type;
        private bool isCompleted;

        public RegionSelectStep(TimeFrameViewModel owner, RegionType type, Color color)
        {
            this.owner = owner;
            this.type = type;
            title = type.ToString();
            this.color = color;
        }

        public RegionType Type => type;

        public string Title => title;

        public Color Color => color;

        public Brush Brush => new SolidColorBrush(Color);

        public bool IsCompleted { get; set; } = false;

        public static IEnumerable<RegionSelectStep> GetStandardSteps(
            TimeFrameViewModel owner)
        {
            yield return new RegionSelectStep(owner, RegionType.VP, Colors.Red);
            yield return new RegionSelectStep(owner, RegionType.PreUES, Colors.Yellow);
            yield return new RegionSelectStep(owner, RegionType.PostUES, Colors.Cyan);
            yield return new RegionSelectStep(owner, RegionType.TB, Colors.Orange);
            yield return new RegionSelectStep(owner, RegionType.HP, Colors.Green);
            yield return new RegionSelectStep(owner, RegionType.UES, Colors.Purple);
        }
    }
}