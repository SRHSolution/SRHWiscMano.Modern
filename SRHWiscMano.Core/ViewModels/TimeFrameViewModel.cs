using CommunityToolkit.Mvvm.ComponentModel;
using NodaTime;
using OxyPlot;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.ViewModels
{
    /// <summary>
    /// 센서 데이터에서 분석을 위한 Snapshot 을 지정하고 이를 View 표기하기 위한 ViewModel
    /// </summary>
    public partial class TimeFrameViewModel : ViewModelBase, ITimeFrame
    {
        private readonly ITimeFrame timeFrame;
        
        public TimeFrameViewModel(ITimeFrame timeFrame)
        {
            this.timeFrame = timeFrame;
        }

        [ObservableProperty] private PlotModel framePlotModel;

        public string Id => timeFrame.Id;
        public IExamination Data { get; }
        public string Text { get; }
        public Instant Time { get; }
        public Range<int> SensorRange { get; }
        public int? VPUpperBound { get; }
        public int? UesLowerBound { get; }
        
        [ObservableProperty] private bool isSelected;
        public bool NormalEligible { get; }

        public ITimeFrameLabels Labels { get; }
        public IReadOnlyList<IRegion> Regions { get; }
        public RegionsVersionType RegionsVersionType { get; }
    }
}