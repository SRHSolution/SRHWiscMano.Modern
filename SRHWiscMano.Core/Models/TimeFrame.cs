using NodaTime;
using SRHWiscMano.Core.Helpers;

namespace SRHWiscMano.Core.Models
{
    public class TimeFrame : ITimeFrame
    {
        public TimeFrame()
        {
        }

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
            Id = id;
            Data = data;
            Text = text;
            Time = time;
            SensorRange = sensorRange;
            VPUpperBound = vpUpperBound;
            UesLowerBound = uesLowerBound;
            IsSelected = isSelected;
            NormalEligible = normalEligible;
            Labels = labels;
            Regions = regions;
            RegionsVersionType = regionsVersionType;
        }

        public string Id { get; }
        public IExamination Data { get; }
        public string Text { get; }
        public Instant Time { get; }
        public Range<int> SensorRange { get; }
        public int? VPUpperBound { get; }
        public int? UesLowerBound { get; }
        public bool IsSelected { get; }
        public bool NormalEligible { get; }
        public ITimeFrameLabels Labels { get; }
        public IReadOnlyList<IRegion> Regions { get; }
        public RegionsVersionType RegionsVersionType { get; }
    }
}
