using System.ComponentModel;
using System.Net.Mime;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using MoreLinq;
using NLog;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using SRHWiscMano.Core.Data;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;
using Range = SRHWiscMano.Core.Helpers.Range;

namespace SRHWiscMano.Core.ViewModels
{
    /// <summary>
    /// 센서 데이터에서 분석을 위한 TimeFrame 을 지정하고 이를 Heatmap View 표기하기 위한 ViewModel
    /// </summary>
    public partial class TimeFrameViewModel : ViewModelBase
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

        private IReadOnlyList<RegionSelectStep> regionSelectSteps;

        public IReadOnlyList<RegionSelectStep> RegionSelectSteps => regionSelectSteps;

        public bool AllStepsAreCompleted => regionSelectSteps.All(s => s.IsCompleted);

        public bool NoStepsAreCompleted => regionSelectSteps.All(s => !s.IsCompleted);

        /// <summary>
        /// Label을 Editing 하는 매개값
        /// </summary>
        [ObservableProperty] private string labelEdit;

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                SetProperty(ref isSelected, value);
                this.Data.IsSelected = value;
            }
        }

        [ObservableProperty] private bool isEditing = false;
        private readonly IDisposable subscribeDispose;
        public static OxyPalette SelectedPalette { get; private set; }

        public TimeFrameViewModel(ITimeFrame data)
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
            plotData = Data.IntpFrameSamples.ConvertToDoubleArray();
            // Heamap Series를 추가한ㄷ
            PlotDataUtils.AddHeatmapSeries(plotModel, plotData);
            // Graph를 위한 Axes를 등록한다
            AddAxes(plotModel, plotData.GetLength(0), plotData.GetLength(1));
            FramePlotModel = plotModel;

            // SourceList의 Connect를 등록하기 전에 이미 있던 Items 에 대해서는 별도로 Draw를 수행한다.
            foreach (var oldRegion in Data.Regions.Items)
            {
                DrawRegionAnnotation(oldRegion);
            }

            subscribeDispose = Data.Regions.Connect().Subscribe(HandleRegionList);
            regionSelectSteps = RegionSelectStep.GetStandardSteps(this).ToList();

            // Heatmap의 색상 Pallete가 변경시 업데이트를 위한 handler를 등록한다
            WeakReferenceMessenger.Default.Register<PaletteChangedMessageMessage>(this, OnPaletteChange);
        }

        public void Dispose()
        {
            subscribeDispose?.Dispose();
            
            DetachView();
            FramePlotModel = null;
            FramePlotController = null;
        }

        /// <summary>
        /// Heatmap을 위한 Axes를 등록한다
        /// </summary>
        /// <param name="model"></param>
        /// <param name="xSize"></param>
        /// <param name="ySize"></param>
        private void AddAxes(PlotModel model, int xSize, int ySize)
        {
            foreach (var axis in model.Axes)
            {
                model.Axes.Remove(axis);
            }

            model.Axes.Add(new LinearColorAxis
            {
                Position = AxisPosition.None,
                Palette = SelectedPalette,
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
                StartPosition = 1,
                EndPosition = 0,
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
                LabelFormatter = value => $"{(value / 100).ToString()}",
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
        /// Region List 추가 삭제에 대한 Annotation 처리를 수행한다.
        /// </summary>
        /// <param name="changeSet"></param>
        private void HandleRegionList(IChangeSet<IRegion> changeSet)
        {
            foreach (var change in changeSet)
            {
                switch (change.Reason)
                {
                    case ListChangeReason.Add:
                    {
                        var region = change.Item.Current;
                        DrawRegionAnnotation(region);

                        try
                        {
                            RegionSelectSteps.FirstOrDefault(stp => stp.Type == region.Type).IsCompleted = true;
                        }
                        catch (Exception)
                        {
                            logger.Log(LogLevel.Error, $"{region.Type} is not valid type");
                        }

                        break;
                    }
                    case ListChangeReason.Remove:
                    {
                        var region = change.Item.Current;
                        var itemToRemove =
                            FramePlotModel.Annotations.Single(ann =>
                                Equals(ann.Tag.ToString(), region.Type.ToString()));
                        FramePlotModel.Annotations.Remove(itemToRemove);
                        FramePlotModel.InvalidatePlot(false);

                        RegionSelectSteps.First(stp => stp.Type == region.Type).IsCompleted = false;
                        break;
                    }
                    case ListChangeReason.RemoveRange:
                    {
                        var rangeToRemove = change.Range;
                        foreach (var region in rangeToRemove)
                        {
                            try
                            {
                                var itemToRemove =
                                    FramePlotModel.Annotations.Single(ann =>
                                        Equals(ann.Tag.ToString(), region.Type.ToString()));
                                FramePlotModel.Annotations.Remove(itemToRemove);
                            }
                            catch (Exception)
                            {
                            }

                            RegionSelectSteps.First(stp => stp.Type == region.Type).IsCompleted = false;
                        }

                        FramePlotModel.InvalidatePlot(false);

                        break;
                    }

                    case ListChangeReason.Clear:
                    {
                        FramePlotModel.Annotations.Clear();
                        FramePlotModel.InvalidatePlot(false);
                        RegionSelectSteps.ForEach(stp => stp.IsCompleted = false);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 입력받은 region 정보에 따라 Rect Annotation를 그린다.
        /// </summary>
        /// <param name="region"></param>
        private void DrawRegionAnnotation(IRegion region)
        {
            var rangeXStart = region.TimeRange.Start.Minus(Data.TimeRange().Start)
                .TotalMilliseconds / 10;
            var rangeXEnd = region.TimeRange.End.Minus(Data.TimeRange().Start)
                                .TotalMilliseconds /
                            10;

            var rangeYTop = (region.SensorRange.Lesser - 0.5) * Data.ExamData.InterpolationScale;
            var rangeYBottom = (region.SensorRange.Greater + 0.5) * Data.ExamData.InterpolationScale;
            var regColor = RegionSelectStep.GetStandardSteps(null).Where(stp => stp.Type == region.Type)
                .Select(stp => stp.Color).FirstOrDefault(Colors.Black);

            var rectangleAnnotation = new RectangleAnnotation
            {
                MinimumX = rangeXStart,
                MaximumX = rangeXEnd,
                MinimumY = rangeYBottom,
                MaximumY = rangeYTop,
                Fill = OxyColors.Transparent,
                Stroke = OxyColor.FromArgb(regColor.A, regColor.R, regColor.G, regColor.B),
                StrokeThickness = 2,
                Tag = region.Type.ToString(),
            };

            FramePlotModel.Annotations.Add(rectangleAnnotation);
            FramePlotModel.InvalidatePlot(false);
        }

        /// <summary>
        /// PlotModel을 그리기 위한 Palette가 변경된 이벤트를 처리한다
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="arg"></param>
        private void OnPaletteChange(object recipient, PaletteChangedMessageMessage arg)
        {
            SelectedPalette = arg.Value.palette;

            var mainColorAxis = FramePlotModel.Axes.OfType<LinearColorAxis>().FirstOrDefault();
            mainColorAxis.Palette = arg.Value.palette;
            mainColorAxis.Minimum = arg.Value.Minimum; // 최소 limit 값
            mainColorAxis.Maximum = arg.Value.Maximum; // 최대 limit 값
            mainColorAxis.HighColor = arg.Value.HighColor; // OxyColors.White,
            mainColorAxis.LowColor = arg.Value.LowColor;

            FramePlotModel.InvalidatePlot(false);
        }

        /// <summary>
        /// TimeFrame의 시간을 조정한다
        /// </summary>
        /// <param name="delta"></param>
        public void AdjustTimeInMs(long delta)
        {
            Data.UpdateTime(Data.Time.Plus(Duration.FromMilliseconds(delta)));
            RefreshPlotData();

            logger.Trace($"Adjust {Data.Text} {delta}msec");
        }

        /// <summary>
        /// Plot을 다시 그린다
        /// </summary>
        public void RefreshPlotData()
        {
            var heatmap = framePlotModel.Series.OfType<HeatMapSeries>().FirstOrDefault();
            var yAxis = framePlotModel.Axes.First(ax => ax.Tag == "Y");

            // 데이터의 interpolation scale값을 찾는다
            var intpScale = (Data.IntpFrameSamples[0].DataSize - 1) / (Data.FrameSamples[0].DataSize - 1);

            // Sensor Bounds 영역만을 표시하도록 한다
            yAxis.Minimum = Data.MinSensorBound * intpScale;
            yAxis.Maximum = Data.MaxSensorBound * intpScale + 1;

            heatmap.Data = Data.IntpFrameSamples.ConvertToDoubleArray();
            heatmap.X0 = 0;
            heatmap.X1 = Data.IntpFrameSamples.Count - 1;
            heatmap.Y0 = 0;
            heatmap.Y1 = Data.IntpFrameSamples[0].DataSize - 1;

            framePlotModel.InvalidatePlot(false);

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

        /// <summary>
        /// PlotModel이 연결되어 있던 PlotView와의 연결을 끊는다.
        /// PlotModel은 한개의 PlotView에만 연결되어 사용할 수 있기 때문이다.
        /// </summary>
        public void DetachView()
        {
            ((IPlotModel)this.framePlotModel)?.AttachPlotView(null);
        }
    }
}