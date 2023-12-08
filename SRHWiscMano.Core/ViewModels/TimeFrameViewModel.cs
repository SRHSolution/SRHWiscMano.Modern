using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
            this.Text = timeFrame.Text;
            if (timeFrame.Text.Contains("cc"))
            {
                this.Volume = timeFrame.Text.Trim().Split("cc")[0];
            }
            else
            {
                this.Volume = timeFrame.Text;
            }
        }

        [ObservableProperty] private PlotModel framePlotModel;

        public string Id => timeFrame.Id;
        public IExamination Data { get; }

        [ObservableProperty] private string text;
        [ObservableProperty] private string volume;
        
        [ObservableProperty] private string labelEdit;
        public Instant Time { get; }
        public Range<int> SensorRange { get; }
        public int? VPUpperBound { get; }
        public int? UesLowerBound { get; }
        
        [ObservableProperty] private bool isSelected;
        public bool NormalEligible { get; }

        public ITimeFrameLabels Labels { get; }
        public IReadOnlyList<IRegion> Regions { get; }
        public RegionsVersionType RegionsVersionType { get; }

        [ObservableProperty] private bool isEditing = false;

        [RelayCommand]
        private void EditLabel()
        {
            IsEditing = true;
            LabelEdit = Volume;
        }

        [RelayCommand]
        private void CommitEditLabel()
        {
            Volume = LabelEdit;
            var labelTag = Text.Split("cc")[1];
            Text = Volume + "cc " + labelTag;
            timeFrame.Text = Text;
            IsEditing = false;
        }
    }
}