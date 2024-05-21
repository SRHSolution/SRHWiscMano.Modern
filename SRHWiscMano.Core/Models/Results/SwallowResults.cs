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

        /// <summary>
        /// Pressure 가 VP 영역에서 최대 값을 갖는 Sample 에 대해서 각 영역에 대한 value를 list로 갖는다. (필요에따라 interpolataion 값으로 갖는다, feat SectionsUsingTBAndHP)
        /// </summary>
        public IReadOnlyList<T> PressureAtVPMax { get; set; }

        /// <summary>
        /// Pressure 가 TB 영역에서 최대 값을 갖는 Sample 에 대해서 각 영역에 대한 value를 list로 갖는다.
        /// </summary>
        public IReadOnlyList<T> PressureAtTBMax { get; set; }


        public IReadOnlyList<T> PressureGradient { get; set; }

        /// <summary>
        /// VP 가 시작한 시간부터 PostUES의 focal point의 시간까지의 범위로 계산한다
        /// </summary>
        public T TotalSwallowDuration { get; set; }

        public T TotalPharyngealPressure { get; set; }
    }
}