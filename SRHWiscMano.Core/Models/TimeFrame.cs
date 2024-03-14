using NodaTime;
using SRHWiscMano.Core.Helpers;
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.Core.Models
{
    public class TimeFrame : ITimeFrame
    {
        private static int GuidId = 0;
        public int Id { get; }

        public double[,] ExamData { get; private set; }

        public string Text { get; set; }
        
        public Instant Time { get; private set; }
        
        /// <summary>
        /// PlotData가 포함할 시간크기
        /// </summary>
        public double TimeDuration { get; }

        public int InterpolateScale { get; set; }

        /// <summary>
        /// Plot을 그리기 위한 실제 데이터
        /// </summary>
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
            Id = Interlocked.Increment(ref GuidId);
            Text = text;
            Time = time;
            PlotData = plotData;
        }


        public TimeFrame(string text, Instant time, double timeDuration, double[,] examData, int interpolateScale)
        {
            Id = Interlocked.Increment(ref GuidId);
            Text = text;
            Time = time;
            TimeDuration = timeDuration;
            InterpolateScale = interpolateScale;
            this.ExamData = examData;
            
            UpdateTime(Time);
        }

        /// <summary>
        /// TimeFrame 의 지정된 시간을 변경한다.
        /// 지정된 시간에서 +/- duration 의 간격에 대해 PlotData를 업데이트 한다.
        /// </summary>
        /// <param name="newTime"></param>
        public void UpdateTime(Instant newTime)
        {
            var startRow = (int)Math.Round(newTime.ToUnixTimeMilliseconds() - TimeDuration / 2) / 10;
            var endRow = (int)Math.Round(newTime.ToUnixTimeMilliseconds() + TimeDuration / 2) / 10;
            PlotData = PlotDataUtils.CreateSubRange(this.ExamData, startRow, endRow-1, 0, this.ExamData.GetLength(1)-1, this.InterpolateScale);
            Time = newTime;
        }

        public object Clone()
        {
            return new TimeFrame(this.Text, this.Time, this.TimeDuration, (double[,])ExamData.Clone(), InterpolateScale);
        }
    }
}
