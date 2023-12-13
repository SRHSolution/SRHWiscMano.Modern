using NodaTime;
using SRHWiscMano.Core.Helpers;

namespace SRHWiscMano.Core.Models
{
    public class TimeFrame : ITimeFrame
    {
        public string Id { get; }

        private readonly double[,] examPlotData;
        
        public string Text { get; set; }
        
        public Instant Time { get; private set; }
        
        public double TimeDuration { get; }

        public double TimeTimeDuration { get; }

        public double[,] PlotData { get; private set; }

        [Obsolete("ManoViewer 의 origin 구성")]
        public TimeFrame(
            string id,  // Model id, 필요 없는듯
            IExamination data,  // 원본데이터 이지만, shared full 데이터를 참조할 예정이므로..?
            string text,    // note의 label text
            Instant time,   // note의 time 
            Range<int> sensorRange, // 기본적인 sensor range 를 고정으로 할 수 있는데..
            int? vpUpperBound,  // sensor upper bound
            int? uesLowerBound, // sensor lower bound
            bool isSelected,    // viewmodel 에서의 check 인데.. 여기에서는 필요 없지
            bool normalEligible,    // 분석 기법을 위한 값이므로.. 우선 필요 없음
            ITimeFrameLabels labels,    // 위에서 note 의 정보와 중복임
            IReadOnlyList<IRegion> regions, // Analysis 에서 수행하는 각 영역에 대한 정보를 저장하고 있음
            RegionsVersionType regionsVersionType) // region 버전
        {
            var _id = id;
            var Data = data;
            var Text = text;
            var Time = time;
            var SensorRange = sensorRange;
            var VPUpperBound = vpUpperBound;
            var UesLowerBound = uesLowerBound;
            var IsSelected = isSelected;
            var NormalEligible = normalEligible;
            var Labels = labels;
            var Regions = regions;
            var RegionsVersionType = regionsVersionType;
        }


        public TimeFrame(string text, Instant time, double[,] plotData)
        {
            Text = text;
            Time = time;
            PlotData = plotData;
        }

        public TimeFrame(string text, Instant time, double timeDuration, double[,] examPlotData)
        {
            Text = text;
            Time = time;
            TimeTimeDuration = timeDuration;
            this.examPlotData = examPlotData;
            
            UpdateTime(Time);
        }

        public void UpdateTime(Instant newTime)
        {
            var startRow = (int)Math.Round(newTime.ToUnixTimeMilliseconds() - TimeTimeDuration / 2) / 10;
            var endRow = (int)Math.Round(newTime.ToUnixTimeMilliseconds() + TimeTimeDuration / 2) / 10;
            PlotData = PlotDataUtils.CreateSubRange(examPlotData, startRow, endRow-1, 0, examPlotData.GetLength(1)-1);
            Time = newTime;
        }
    }
}
