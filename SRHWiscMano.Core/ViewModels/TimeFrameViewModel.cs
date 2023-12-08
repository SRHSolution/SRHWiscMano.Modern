using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using OxyPlot;
using OxyPlot.Axes;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.ViewModels
{
    /// <summary>
    /// 센서 데이터에서 분석을 위한 Snapshot 을 지정하고 이를 View 표기하기 위한 ViewModel
    /// </summary>
    public partial class TimeFrameViewModel : ViewModelBase
    {
        private readonly ITimeFrame timeFrame;

        [ObservableProperty] private PlotModel framePlotModel;

        /// <summary>
        /// TimeFrame의 메인 Label
        /// </summary>
        [ObservableProperty] private string label;

        /// <summary>
        /// Label에서 포함되어 있는 Volume 정보
        /// </summary>
        [ObservableProperty] private string volume;

        /// <summary>
        /// Label을 Editing 하는 매개값
        /// </summary>
        [ObservableProperty] private string labelEdit;

        public Instant Time => timeFrame.Time;


        public TimeFrameViewModel(ITimeFrame timeFrame, FrameNote frameNote)
        {
            this.timeFrame = timeFrame;
            this.Label = timeFrame.Text;
            if (Label.Contains("cc"))
            {
                this.Volume = Label.Trim().Split("cc")[0];
            }
            else
            {
                this.Volume = Label;
            }

            var plotModel = new PlotModel();
            PlotDataUtils.AddHeatmapSeries(plotModel, timeFrame.PlotData);
            AddAxes(plotModel, timeFrame.PlotData.GetLength(0), timeFrame.PlotData.GetLength(1));
            FramePlotModel = plotModel;
        }


        private void AddAxes(PlotModel model, int xSize, int ySize)
        {
            foreach (var axis in model.Axes)
            {
                model.Axes.Remove(axis);
            }

            model.Axes.Add(new LinearColorAxis
            {
                Position = AxisPosition.None,
                Palette = OxyPalettes.Jet(256),
                // HighColor = SelectedPalette.Colors.Last(), // OxyColors.White,
                // LowColor = SelectedPalette.Colors.First(),
                RenderAsImage = false,
                AbsoluteMinimum = -5,
                AbsoluteMaximum = 200,
                Tag = "Color"
            });

            model.Axes.Add(new LinearAxis()
            {
                IsPanEnabled = false,
                IsZoomEnabled = false,
                Position = AxisPosition.Left,
                MaximumPadding = 0,
                MinimumPadding = 0,
                Minimum = 0, // 초기 시작값
                Maximum = ySize - 1, // 초기 최대값
                AbsoluteMinimum = 0, // Panning 최소값
                AbsoluteMaximum = ySize - 1, // Panning 최대값
                IsAxisVisible = false,
                Tag = "Y"
            });

            // X-Axis
            model.Axes.Add(new LinearAxis()
            {
                // IsZoomEnabled = false,
                LabelFormatter = value => $"{(value / 1000).ToString()}",
                Position = AxisPosition.Bottom,
                MinimumPadding = 0,
                Minimum = 0,
                Maximum = xSize - 1,
                MajorStep = xSize,
                MinorStep = xSize, // 최대 범위를 입력하여 MinorStep 이 표시되지 않도록 한다
                AbsoluteMinimum = 0,
                AbsoluteMaximum = xSize - 1,
                IsAxisVisible = false,
                Tag = "X"
            });
        }

        [ObservableProperty] private bool isSelected;
        [ObservableProperty] private bool isEditing = false;


        /// <summary>
        /// Label을 Editing 상태로 전환
        /// </summary>
        [RelayCommand]
        private void EditLabel()
        {
            IsEditing = true;
            LabelEdit = Volume;
        }

        /// <summary>
        /// Label Editing 완료
        /// </summary>
        [RelayCommand]
        private void CommitEditLabel()
        {
            Volume = LabelEdit;
            var labelTag = Label.Split("cc")[1];
            Label = Volume + "cc " + labelTag;
            timeFrame.Text = Label;
            IsEditing = false;
        }
    }
}