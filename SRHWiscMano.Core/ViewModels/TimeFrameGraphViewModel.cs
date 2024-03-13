using System.Net.Mime;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NLog;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using SRHWiscMano.Core.Data;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.ViewModels
{
    /// <summary>
    /// 센서 데이터에서 분석을 위한 Snapshot 을 지정하고 이를 Graph View로 표기하기 위한 ViewModel
    /// </summary>
    public partial class TimeFrameGraphViewModel : ViewModelBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        // private readonly ITimeFrame timeFrame;

        public ITimeFrame Data { get; }

        public static OxyPalette SelectedPalette { get; private set; }

        [ObservableProperty] private PlotModel framePlotModel;
        [ObservableProperty] private PlotController framePlotController;

        public int Id { get; }

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

        [ObservableProperty] private bool isSelected;

        [ObservableProperty] private bool isEditing = false;

        public int SensorRange { get; private set; } = 10;

        public Instant Time { get; private set; }


        public TimeFrameGraphViewModel(ITimeFrame data)
        {
            this.Data = data;
            this.Id = data.Id;
            this.Time = data.Time;
            this.Label = data.Text;
            if (Label.Contains("cc"))
            {
                this.Volume = Label.Trim().Split("cc")[0];
            }
            else
            {
                this.Volume = Label;
            }

            var plotModel = new PlotModel();
            AddAxes(plotModel, data.PlotData.GetLength(0), data.PlotData.GetLength(1));
            AddLineSeries(plotModel, data.PlotData);
            FramePlotModel = plotModel;
        }

        /// <summary>
        /// Sensor 데이터를 Graph Series 로 그리는 함수
        /// </summary>
        /// <param name="model"></param>
        /// <param name="plotData"></param>
        private void AddLineSeries(PlotModel model, double[,] plotData)
        {
            var frameCount = plotData.GetLength(0) - 1;
            var sensorCount = plotData.GetLength(1) - 1;

            var valueRange = plotData.Max2D() - plotData.Min2D();
            var valueScale = SensorRange / valueRange;

            for (int colId = 0; colId < sensorCount; colId++)
            {
                var lineSeries = new LineSeries();
                lineSeries.LineStyle = LineStyle.Solid;
                lineSeries.StrokeThickness = 1;
                lineSeries.Color = OxyColors.Gray;

                for (int rowId = 0; rowId < frameCount; rowId++)
                {
                    lineSeries.Points.Add(new OxyPlot.DataPoint(rowId, (colId * SensorRange) + plotData[rowId, colId]* valueScale*2));
                }

                model.Series.Add(lineSeries);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model">그릴 Model</param>
        /// <param name="xSize">Time frames</param>
        /// <param name="ySize">센서 갯수</param>
        /// <param name="sensorRange">Y축 센서에서 값을 표시 하기 위한 Range 값, 입력된 값의 크기만큼 하나의 센서의 데이터를 표현하도록 한다</param>
        private void AddAxes(PlotModel model, int xSize, int ySize)
        {
            foreach (var axis in model.Axes)
            {
                model.Axes.Remove(axis);
            }

            model.Axes.Add(new LinearAxis()
            {
                IsPanEnabled = false,
                IsZoomEnabled = false,
                Position = AxisPosition.Left,
                MaximumPadding = 0,
                MinimumPadding = 0,
                Minimum = 0, // 초기 시작값
                Maximum = (ySize * SensorRange) - 1, // 초기 최대값
                AbsoluteMinimum = 0, // Panning 최소값
                AbsoluteMaximum = (ySize * SensorRange) - 1, // Panning 최대값
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
            var labelTag = Label.Split("cc")[1].Trim();
            Label = Volume + "cc" + labelTag;
            Data.Text = Label;
            IsEditing = false;
        }

        public void RefreshPlotData()
        {
            // 이전과 동일하면 skip
            if (Time.Equals(Data.Time))
                return;

            var heatmap = framePlotModel.Series.OfType<HeatMapSeries>().FirstOrDefault();
            heatmap.Data = Data.PlotData;
            heatmap.X0 = 0;
            heatmap.X1 = Data.PlotData.GetLength(0) - 1;
            heatmap.Y0 = 0;
            heatmap.Y1 = Data.PlotData.GetLength(1) - 1;
            framePlotModel.InvalidatePlot(true);

            this.Time = Data.Time;
        }

        public void AdjustTimeInMs(long delta)
        {
            Data.UpdateTime(Data.Time.Plus(Duration.FromMilliseconds(delta)));
            RefreshPlotData();

            logger.Trace($"Adjust {Data.Text} {delta}msec");
        }
    }
}