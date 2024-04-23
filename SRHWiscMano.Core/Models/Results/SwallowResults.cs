using System.Runtime.CompilerServices;

namespace SRHWiscMano.Core.Models.Results
{
    public class SwallowResults<T>
    {
        public SwallowResults()
        {
            VP = new ResultParameters<T>();
            TB = new ResultParameters<T>();
            HP = new ResultParameters<T>();
            PreUES = new ResultParameters<T>();
            PostUES = new ResultParameters<T>();
            UES = new UesResultParameters<T>();
            MaxPressures = new List<T>();
            PressureAtVPMax = new List<T>();
            PressureAtTBMax = new List<T>();
            PressureGradient = new List<T>();
        }


        public ResultParameters<T> VP { get; set; }

        public ResultParameters<T> TB { get; set; }

        public ResultParameters<T> HP { get; set; }

        public ResultParameters<T> PreUES { get; set; }

        public ResultParameters<T> PostUES { get; set; }

        public UesResultParameters<T> UES { get; set; }

        public IReadOnlyList<T> MaxPressures { get; set; }

        public IReadOnlyList<T> PressureAtVPMax { get; set; }

        public IReadOnlyList<T> PressureAtTBMax { get; set; }

        public IReadOnlyList<T> PressureGradient { get; set; }

        public T TotalSwallowDuration { get; set; }

        public T TotalPharyngealPressure { get; set; }
    }
}