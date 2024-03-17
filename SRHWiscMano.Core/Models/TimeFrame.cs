using NodaTime;
using SRHWiscMano.Core.Helpers;
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.Core.Models
{
    /// <summary>
    /// Duration 시간동안에 저장된 TimeSample을을 갖는 클래스
    /// </summary>
    public class TimeFrame : ITimeFrame
    {
        private static int GuidId = 0;
        public int Id { get; }

        public IReadOnlyList<TimeSample> OwnerSamples { get; set; }
        public IReadOnlyList<TimeSample> IntpSamples { get; private set; }

        public IReadOnlyList<TimeSample> FrameSamples { get; private set; }
        public IReadOnlyList<TimeSample> IntpFrameSamples { get; private set; }

        public string Text { get; set; }

        public Instant Time { get; private set; }

        /// <summary>
        /// Samples 포함할 시간크기
        /// </summary>
        public double TimeDuration { get; }

        public TimeFrame(string text, Instant time, double timeDuration, IReadOnlyList<TimeSample> ownerSamples,
            IReadOnlyList<TimeSample> intpSamples)
        {
            Id = Interlocked.Increment(ref GuidId);
            Text = text;
            Time = time;
            TimeDuration = timeDuration;
            OwnerSamples = ownerSamples;
            IntpSamples = intpSamples;

            UpdateTime(Time);
        }


        /// <summary>
        /// TimeFrame 의 지정된 시간을 변경한다.
        /// 지정된 시간에서 +/- duration 의 간격에 대해 PlotData를 업데이트 한다.
        /// </summary>
        /// <param name="newTime"></param>
        public void UpdateTime(Instant newTime)
        {
            var startTime = newTime.Plus(-Duration.FromMilliseconds(TimeDuration) / 2);
            var endTime = newTime.Plus(Duration.FromMilliseconds(TimeDuration) / 2);
            FrameSamples = OwnerSamples.GetSubSamples(startTime, endTime);
            IntpFrameSamples = IntpSamples.GetSubSamples(startTime, endTime);

            Time = newTime;
        }

        public object Clone()
        {
            return new TimeFrame(Text, Time, TimeDuration, OwnerSamples, IntpSamples);
        }


        [Obsolete("ManoViewer 의 origin 구성")]
        public TimeFrame(
            string id, // Model id, 필요 없는듯
            IExamination data, // 원본데이터 이지만, shared full 데이터를 참조할 예정이므로..?
            string text, // note의 label text
            Instant time, // note의 time 
            Range<int> sensorRange, // 기본적인 sensor range 를 고정으로 할 수 있는데..
            int? vpUpperBound, // sensor upper bound
            int? uesLowerBound, // sensor lower bound
            bool isSelected, // viewmodel 에서의 check 인데.. 여기에서는 필요 없지
            bool normalEligible, // 분석 기법을 위한 값이므로.. 우선 필요 없음
            ITimeFrameLabels labels, // 위에서 note 의 정보와 중복임
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
    }
}