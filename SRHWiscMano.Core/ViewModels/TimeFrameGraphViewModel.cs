using System;
using System.Net.Mime;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
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
    /// 센서 데이터에서 분석을 위한 TimeFrame 을 지정하고 이를 Graph View로 표기하기 위한 ViewModel
    /// </summary>
    public partial class TimeFrameGraphViewModel : ViewModelBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public int Id { get; }
        public ITimeFrame Data { get; }

        /// <summary>
        /// TimeFrame의 중심위치 시간
        /// </summary>
        public Instant Time { get; private set; }

        /// <summary>
        /// TimeFrame의 메인 Label
        /// </summary>
        [ObservableProperty] private string label;

        /// <summary>
        /// Label에서 포함되어 있는 Volume 정보
        /// </summary>
        [ObservableProperty] private string volume;

        [ObservableProperty] private PlotModel framePlotModel;
        [ObservableProperty] private PlotController framePlotController;

        private double[,] plotData;

        /// <summary>
        /// Label을 Editing 하는 매개값
        /// </summary>
        [ObservableProperty] private string labelEdit;

        [ObservableProperty] private bool isSelected;

        [ObservableProperty] private bool isEditing = false;
        private readonly IDisposable subscribeDipose;


        public static OxyPalette SelectedPalette { get; private set; }

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
            //TimeFrame의 원본 데이터 값을 Plot을 위한 Double Array 로 저장한다
            plotData = Data.FrameSamples.ConvertToDoubleArray();
            // Line graph를 위한 Axes를 등록한다
            AddAxes(plotModel, plotData.GetLength(0), plotData.GetLength(1));
            // Sensor Bounds 에 맞춰서 Line 데이터를 등록한다
            // UpdateLineSeries(plotModel, plotData, Data.MinSensorBound, Data.MaxSensorBound);
            FramePlotModel = plotModel;

            subscribeDipose = Data.Regions.Connect().Subscribe(HandleRegionList);
        }


        public void Dispose()
        {
            subscribeDipose?.Dispose();
        }

        private void HandleRegionList(IChangeSet<IRegion> changeSet)
        {
            foreach (var change in changeSet)
            {
                switch (change.Reason)
                {
                    case ListChangeReason.AddRange:
                    {
                        var itemsToAdd = change.Range;
                        foreach (var region in itemsToAdd)
                        {
                            DrawRegionAnnotation(region);
                            DrawStatisticsLine(region);
                        }

                        FramePlotModel.InvalidatePlot(true);
                        break;
                    }
                    case ListChangeReason.Add:
                    {
                        var region = change.Item.Current;
                        DrawRegionAnnotation(region);
                        DrawStatisticsLine(region);
                        FramePlotModel.InvalidatePlot(true);
                        break;
                    }
                    case ListChangeReason.Remove:
                    {
                        var region = change.Item.Current;
                        var itemToRemove =
                            FramePlotModel.Series.Where(sr => sr.Tag != null).Single(ann =>
                                Equals(ann.Tag.ToString(), "Stat_" + region.Type.ToString()));
                        FramePlotModel.Series.Remove(itemToRemove);
                        FramePlotModel.InvalidatePlot(true);

                        break;
                    }
                    case ListChangeReason.RemoveRange:
                    {
                        var rangeToRemove = change.Range;
                        foreach (var region in rangeToRemove)
                        {
                            var itemToRemove =
                                FramePlotModel.Series.Where(sr => sr.Tag != null).Single(ann =>
                                    Equals(ann.Tag.ToString(), "Stat_" + region.Type.ToString()));
                            FramePlotModel.Series.Remove(itemToRemove);
                            FramePlotModel.InvalidatePlot(true);
                        }

                        break;
                    }

                    case ListChangeReason.Clear:
                    {
                        var itemsToRemove = FramePlotModel.Series
                            .Where(sr => sr.Tag != null && sr.Tag.ToString().Contains("Stat")).ToList();
                        foreach (var itemToRemove in itemsToRemove)
                        {
                            FramePlotModel.Series.Remove(itemToRemove);
                        }

                        FramePlotModel.InvalidatePlot(true);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Region 에 들어있는 SensorRange에서의 Max 라인 그리기
        /// </summary>
        /// <param name="region"></param>
        private void DrawStatisticsLine(IRegion region)
        {
            var maxValues = region.Window.ExamData
                .MaxValueForSensorInTimeRange(region.Window.TimeRange(), region.SensorRange).ToList();
            var frameCount = maxValues.Count;
            var windowValues = region.Window.FrameSamples.ConvertToDoubleArray();
            var valueScale = 1 / (windowValues.Max2D() - windowValues.Min2D());

            var lineColor = RegionSelectStep.GetStandardSteps(null).ToList().Find(r => r.Type == region.Type).Color;

            var lineSeries = new LineSeries();
            lineSeries.LineStyle = LineStyle.Solid;
            lineSeries.StrokeThickness = 2;
            lineSeries.Color = OxyColor.FromArgb(lineColor.A, lineColor.R, lineColor.G, lineColor.B);
            lineSeries.Tag = "Stat_" + region.Type.ToString();

            // Line의 Value 값을 추가한다
            for (int rowId = 0; rowId < frameCount; rowId++)
            {
                // Sensor의 rendering 위치가 반전되어 있으므로 PlotData를 반전하여 그린다
                lineSeries.Points.Add(new DataPoint(rowId,
                    region.SensorRange.Lesser - maxValues[rowId] * valueScale * 1.8));
            }

            FramePlotModel.Series.Add(lineSeries);
        }

        /// <summary>
        /// Poly line 을 그리기 
        /// </summary>
        /// <param name="region"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void DrawRegionAnnotation(IRegion region)
        {
            // throw new NotImplementedException();
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

            // Y-Axis
            model.Axes.Add(new LinearAxis()
            {
                IsPanEnabled = false,
                IsZoomEnabled = false,
                Position = AxisPosition.Left,
                MaximumPadding = 0,
                MinimumPadding = 0,
                StartPosition = 1,
                EndPosition = 0,
                Minimum = 0, // 초기 시작값
                Maximum = ((ySize + 1)), // 초기 최대값
                AbsoluteMinimum = -0, // Panning 최소값
                AbsoluteMaximum = ((ySize + 1)), // Panning 최대값, LineSeries는 데이터를 한 step shift 하여 표시하기 위해 ySize+1 한다
                IsAxisVisible = false,
                Tag = "Y"
            });

            // X-Axis
            model.Axes.Add(new LinearAxis()
            {
                // IsZoomEnabled = false,
                LabelFormatter = value => $"{value / 100}",
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
        /// Sensor 데이터를 Graph Series 로 그린다
        /// </summary>
        /// <param name="model"></param>
        /// <param name="plotData"></param>
        /// <param name="minBound"></param>
        /// <param name="maxBound"></param>
        private void UpdateLineSeries(PlotModel model, double[,] plotData, double minBound, double maxBound)
        {
            var frameCount = plotData.GetLength(0);
            var sensorCount = plotData.GetLength(1);

            var valueRange = plotData.Max2D() - plotData.Min2D();
            var valueScale = 1 / valueRange;

            var colId = 0;
            for (int senId = 0; senId < sensorCount; senId++)
            {
                var lineSeries = new LineSeries();
                lineSeries.LineStyle = LineStyle.Solid;
                lineSeries.StrokeThickness = 1;
                lineSeries.Color = OxyColors.Gray;

                // Line의 Value 값을 추가한다
                for (int rowId = 0; rowId < frameCount; rowId++)
                {
                    // Sensor의 rendering 위치가 반전되어 있으므로 PlotData를 반전하여 그린다
                    lineSeries.Points.Add(new DataPoint(rowId, colId - plotData[rowId, senId] * valueScale * 1.8));
                }

                colId++;
                model.Series.Add(lineSeries);
            }

            var viewYAxis = model.Axes.First(ax => ax.Tag == "Y");
            viewYAxis.Minimum = minBound;
            viewYAxis.Maximum = maxBound;
        }

        public void AddLineSeries(int sensorIndex, List<double> plotData)
        {
            var frameCount = plotData.Count;

            var valueScale = 1 / (plotData.Max() - plotData.Min());

            var lineSeries = new LineSeries();
            lineSeries.LineStyle = LineStyle.Solid;
            lineSeries.StrokeThickness = 2;
            lineSeries.Color = OxyColors.Red;


            // Line의 Value 값을 추가한다
            for (int rowId = 0; rowId < frameCount; rowId++)
            {
                // Sensor의 rendering 위치가 반전되어 있으므로 PlotData를 반전하여 그린다
                lineSeries.Points.Add(new DataPoint(rowId, sensorIndex - plotData[rowId] * valueScale * 1.8));
            }

            FramePlotModel.Series.Add(lineSeries);
            FramePlotModel.InvalidatePlot(true);
        }


        public void AdjustTimeInMs(long delta)
        {
            Data.UpdateTime(Data.Time.Plus(Duration.FromMilliseconds(delta)));
            RefreshPlotData();

            logger.Trace($"Adjust {Data.Text} {delta}msec");
        }

        /// <summary>
        /// Plot을 업데이트한다
        /// </summary>
        public void RefreshPlotData()
        {
            Data.UpdateTime(Data.Time);

            // 기존 데이터를 지운다
            FramePlotModel.Series.Clear();

            // PlotData를 그린다
            plotData = Data.FrameSamples.ConvertToDoubleArray();
            UpdateLineSeries(FramePlotModel, plotData, Data.MinSensorBound, Data.MaxSensorBound);

            // Region 영역에 대한 분석 최대값 line을 그린다
            foreach (var rgn in Data.Regions.Items)
            {
                DrawStatisticsLine(rgn);
            }

            // GDI 를 다시 그린다
            framePlotModel.InvalidatePlot(true);
            this.Time = Data.Time;
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
    }
}