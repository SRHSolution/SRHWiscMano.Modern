using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoreLinq.Extensions;
using NodaTime;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using Shouldly;
using SRHWiscMano.App.Data;
using SRHWiscMano.App.Services;
using SRHWiscMano.Core.Control;
using SRHWiscMano.Core.Data;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.App.ViewModels
{
    public partial class ViewerViewModel : ViewModelBase, IViewerViewModel
    {
        #region Services

        private readonly ILogger<ViewerViewModel> logger;
        private readonly SharedService sharedService;
        private readonly PaletteManager paletteManager;
        private readonly AppSettings settings;

        #endregion

        public IExamination ExamData { get; private set; }

        public ObservableCollection<FrameNote> Notes { get; }


        [ObservableProperty] private PlotModel mainPlotModel;

        [ObservableProperty] private PlotController mainPlotController;

        [ObservableProperty] private PlotModel overviewPlotModel;

        [ObservableProperty] private PlotController overviewPlotController;

        private double[,] fullExamData;
        private double[,] plotSampleData;

        private IDisposable axisChangeObserver = null;
        private readonly SourceCache<ITimeFrame, int> timeFrames;

        [ObservableProperty] private double minSensorData;
        [ObservableProperty] private double maxSensorData;
        [ObservableProperty] private double minSensorRange = 0;
        [ObservableProperty] private double maxSensorRange = 100;

        [ObservableProperty] private long timeDuration = 2000;
        [ObservableProperty] private OxyPalette selectedPalette = OxyPalettes.Hue64;

        [ObservableProperty] private double examSensorSize;

        // Sensor Bound Properties
        [ObservableProperty] private bool pickingSensorBounds = false;
        [ObservableProperty] private Thickness sensorBoundUpperMargin;
        [ObservableProperty] private Thickness sensorBoundLowerMargin;
        [ObservableProperty] private Thickness sensorBoundsSliderMargin;

        [ObservableProperty] private double minSensorBound;
        [ObservableProperty] private double maxSensorBound;


        [ObservableProperty] private double sensorBoundWidth;
        [ObservableProperty] private double sensorBoundHeight;
        [ObservableProperty] private double sensorBoundUpperHeight;
        [ObservableProperty] private double sensorBoundLowerHeight;


        private bool updateSubRange = true;

        public bool UpdateSubRange
        {
            get => updateSubRange;
            set
            {
                SetProperty(ref updateSubRange, value);
                settings.UpdateSubRange = value;
                if (MainPlotModel != null && value == false)
                {
                    var heatMapSeries = MainPlotModel.Series.OfType<HeatMapSeries>().FirstOrDefault();
                    var xAxis = MainPlotModel.Axes.FirstOrDefault(a => a.Position == AxisPosition.Bottom);
                    if (heatMapSeries != null)
                    {
                        heatMapSeries.Data = fullExamData;
                        heatMapSeries.X0 = 0;
                        heatMapSeries.X1 = fullExamData.GetLength(0) - 1;
                        xAxis.Minimum = xAxis.ActualMinimum;
                        xAxis.Maximum = xAxis.ActualMaximum;
                    }
                }
            }
        }

        private string selectedPaletteKey;

        public string SelectedPaletteKey
        {
            get => selectedPaletteKey;
            set
            {
                SetProperty(ref selectedPaletteKey, value);
                UpdatePaletteChanged();
            }
        }

        public Dictionary<string, OxyPalette> Palettes { get; }

        private Color pvBackColor;
        private Color pvForeColor;

        private bool isDataLoaded;

        private bool IsDataLoaded
        {
            get => isDataLoaded;
            set
            {
                isDataLoaded = value;
                FitToScreenCommand.NotifyCanExecuteChanged();
            }
        }

        public ViewerViewModel()
        {
        }

        public ViewerViewModel(ILogger<ViewerViewModel> logger, SharedService sharedService,
            IOptions<AppSettings> settings, PaletteManager paletteManager)
        {
            this.logger = logger;
            this.sharedService = sharedService;
            this.paletteManager = paletteManager;
            this.settings = settings.Value;
            timeFrames = sharedService.TimeFrames;

            timeDuration = this.settings.MainViewFrameRange;

            timeFrames.Connect().Subscribe(HandleTimeFrames);
            sharedService.ExamDataLoaded += SharedService_ExamDataLoaded;

            Palettes = paletteManager.Palettes;

            MaxSensorData = 100;
            MinSensorData = -10;

            MainPlotModel = new PlotModel();
            OverviewPlotModel = new PlotModel();

            pvBackColor = this.settings.BaseBackColor;
            pvForeColor = this.settings.BaseForeColor;
            ApplyThemeToOxyPlots();

            UpdateSubRange = this.settings.UpdateSubRange;

            WeakReferenceMessenger.Default.Register<AppBaseThemeChangedMessage>(this, ThemeChanged);
        }

        private void ThemeChanged(object recipient, AppBaseThemeChangedMessage message)
        {
            if (MainPlotModel != null)
            {
                pvBackColor = message.Value.Item1;
                pvForeColor = message.Value.Item2;
                ApplyThemeToOxyPlots();
            }
        }

        private void ApplyThemeToOxyPlots()
        {
            logger.LogTrace("Apply Theme to OxyPlots");
            MainPlotModel.ApplyTheme(pvBackColor, pvForeColor);
            OverviewPlotModel.ApplyTheme(pvBackColor, pvForeColor);
        }

        



        /// <summary>
        /// SharedService의 TimeFrames에 등록된 데이터를 View에 binding 작업을 수행한다.
        /// </summary>
        /// <param name="changeSet"></param>
        private void HandleTimeFrames(IChangeSet<ITimeFrame, int> changeSet)
        {
            foreach (var change in changeSet)
            {
                switch (change.Reason)
                {
                    case ChangeReason.Add:
                    {
                        var item = change.Current;
                        var msec = item.Time.ToMillisecondsFromEpoch();
                        CreateVLineAnnotation(item, true, MainPlotModel);
                        CreateVLineAnnotation(item, false, OverviewPlotModel);
                        break;
                    }
                    case ChangeReason.Remove:
                    {
                        var mainAnno = MainPlotModel.Annotations.OfType<LineAnnotation>()
                            .Single(item => (int)item.Tag == change.Current.Id);
                        MainPlotModel.Annotations.Remove(mainAnno);

                        var overAnno = OverviewPlotModel.Annotations.OfType<LineAnnotation>()
                            .Single(item => (int)item.Tag == change.Current.Id);
                        OverviewPlotModel.Annotations.Remove(overAnno);

                        logger.LogTrace($"Removed: {change.Current.Text}");
                        break;
                    }
                    case ChangeReason.Moved:
                        break;
                    case ChangeReason.Update:
                    {
                        var xPos = change.Current.Time.ToMillisecondsFromEpoch()/10;
                        var mainAnno = MainPlotModel.Annotations.OfType<LineAnnotation>()
                            .Single(item => (int)item.Tag == change.Current.Id);
                        mainAnno.Text = change.Current.Text;
                        mainAnno.X = xPos;
                        MainPlotModel.InvalidatePlot(false);

                        var overAnno = OverviewPlotModel.Annotations.OfType<LineAnnotation>()
                            .Single(item => (int)item.Tag == change.Current.Id);
                        overAnno.Text = change.Current.Text;
                        overAnno.X = xPos;
                        OverviewPlotModel.InvalidatePlot(false);

                        logger.LogTrace($"Update: {change.Current.Text}");
                        break;
                    }
                    case ChangeReason.Refresh:
                        break;
                }
            }
        }

        /// <summary>
        /// TimeFrame 에 대한 Vertical Line Annotation 을 그리고 Drag를 이용한 제어에 대한 이벤트를 등록한다
        /// </summary>
        /// <param name="timeFrame"></param>
        /// <param name="draggable"></param>
        /// <param name="model"></param>
        private void CreateVLineAnnotation(ITimeFrame timeFrame, bool draggable, PlotModel model)
        {
            var xPos = timeFrame.Time.ToMillisecondsFromEpoch()/10;
            var la = new LineAnnotation
            {
                Type = LineAnnotationType.Vertical,
                X = xPos,
                LineStyle = LineStyle.Solid,
                ClipByYAxis = true,
                Text = timeFrame.Text,
                TextColor = OxyColors.White,
                TextOrientation = AnnotationTextOrientation.Horizontal,
                Tag = timeFrame.Id
            };

            if (draggable)
            {
                model.ShouldNotBeNull("Draggable Line annotation requires plot model");

                la.MouseDown += (s, e) =>
                {
                    if (e.ChangedButton != OxyMouseButton.Left)
                    {
                        return;
                    }

                    la.StrokeThickness *= 5;
                    model.InvalidatePlot(false);
                    e.Handled = true;
                };

                // Handle mouse movements (note: this is only called when the mousedown event was handled)
                la.MouseMove += (s, e) =>
                {
                    la.X = (long)la.InverseTransform(e.Position).X;
                    model.InvalidatePlot(false);
                    e.Handled = true;
                };

                la.MouseUp += (s, e) =>
                {
                    la.StrokeThickness /= 5;
                    model.InvalidatePlot(false);

                    la.X = (long)la.InverseTransform(e.Position).X;
                    timeFrame.UpdateTime(Instant.FromUnixTimeMilliseconds((long)la.X*10));
                    timeFrames.AddOrUpdate(timeFrame);

                    e.Handled = true;
                };
            }

            model!.Annotations.Add(la);
        }


        private void SharedService_ExamDataLoaded(object? sender, EventArgs e)
        {
            LoadExamDataImpl();
        }

        /// <summary>
        /// 로드된 Exam 데이터를 Viewer에 표시하도록 처리한다
        /// </summary>
        private void LoadExamDataImpl()
        {
            var examData = sharedService.ExamData;
            var sensorCount = examData.SensorCount();
            var frameCount = examData.Samples.Count;
            
            MinSensorBound = 0;
            MaxSensorBound = sensorCount;
            ExamSensorSize = sensorCount;

            // 기존의 PlotView를 clear 한 후 ExamData에 대한 PlotModel을 생성해서 입력한다.
            // ((IPlotModel)this.MainPlotModel)?.AttachPlotView(null);

            //Mainview plotmodel, controller 설정
            var mainModel = MainPlotModel;
            fullExamData = sharedService.InterpolatedSamples.ConvertToDoubleArray();

            // 입력받은 Exam 데이터에서 최소 최대 값을 얻어 RangeSlider의 최소/최대 값을 변경한다
            MinSensorData = Math.Floor(fullExamData.Cast<double>().Min());
            MaxSensorData = Math.Ceiling(fullExamData.Cast<double>().Max());

            if (UpdateSubRange)
            {
                plotSampleData = sharedService.InterpolatedSamples.SamplesInTimeRange(Instant.FromUnixTimeMilliseconds(0),
                    Instant.FromUnixTimeMilliseconds(settings.MainViewFrameRange)).ConvertToDoubleArray();
            }
            else
            {
                plotSampleData = fullExamData;
            }

            PlotDataUtils.AddHeatmapSeries(mainModel, plotSampleData);
            AddAxesOnMain(mainModel, frameCount, plotSampleData.GetLength(1));

            // SubRange 업데이트 기능을 위한 이벤트 등록
            var xAxis = mainModel.Axes.First(ax => ax.Tag == "X");
            if (axisChangeObserver != null)
                axisChangeObserver.Dispose();

            // X 축 Axis를 변경하는 이벤트를 처리하는 hanlder를 등록한다
            axisChangeObserver = Observable.FromEvent<EventHandler<AxisChangedEventArgs>, AxisChangedEventArgs>(
                handler => (sender, e) => handler(e),
                handler => xAxis.AxisChanged += handler,
                handler => xAxis.AxisChanged -= handler).Subscribe(OnMainViewAxisChanged);

            MainPlotController = BuildMainController(xAxis);

            //Overview plotmodel, controller 설정
            // ((IPlotModel)this.OverviewPlotModel)?.AttachPlotView(null);
            var overviewModel = OverviewPlotModel;
            PlotDataUtils.AddHeatmapSeries(overviewModel, fullExamData);
            AddAxesOnOverview(overviewModel, frameCount, plotSampleData.GetLength(1));
            OverviewPlotModel = overviewModel;

            OverviewPlotController = BuildOverviewController();

            ApplyThemeToOxyPlots();

            SelectedPaletteKey = paletteManager.SelectedPaletteKey;

            IsDataLoaded = true;
        }

        /// <summary>
        /// MainPlotView를 위한 controller 구성
        /// </summary>
        /// <param name="xAxis"></param>
        /// <returns></returns>
        private PlotController BuildMainController(Axis xAxis)
        {
            var mainController = new PlotController();

            // Bind 하게 되면 기존의 MouseWheel(zoom) 이 삭제된다.
            mainController.BindMouseWheel(new DelegatePlotCommand<OxyMouseWheelEventArgs>((v, c, a) =>
            {
                var range = xAxis.ActualMaximum - xAxis.ActualMinimum;
                var delta = a.Delta > 0 ? -100 : 100;
                var newRange = range + delta;
                var scale = range / newRange;
                var current = xAxis.InverseTransform(a.Position.X);
                xAxis.ZoomAt(scale, current);

                // update view
                var newActualRange = Math.Round(xAxis.ActualMaximum - xAxis.ActualMinimum);
                MainPlotModel.InvalidatePlot(true);
                settings.MainViewFrameRange = TimeDuration = (long)newActualRange * 10;
            }));

            // LineAnnotation을 Panning 하는 MouseManipulator Command를 추가한다.
            // controller에 추가한 command가 완료(mouse up)되면 제거된다. 
            var lineAnnotPan = new DelegatePlotCommand<OxyMouseDownEventArgs>((view, controller, args) =>
                controller.AddMouseManipulator(view, new LineAnnotationManipulator(view)
                {
                    IsVertical = true,
                }, args));
            mainController.BindMouseDown(OxyMouseButton.Left, lineAnnotPan);
            return mainController;
        }

        /// <summary>
        /// OverView를 위한 controller 구성
        /// </summary>
        /// <returns></returns>
        private PlotController BuildOverviewController()
        {
            var overviewController = new PlotController();
            var overviewTrackAt = new DelegatePlotCommand<OxyMouseDownEventArgs>((view, controller, args) =>
            {
                var overviewXAxis = view.ActualModel.Axes.First(ax => ax.Tag == "X");
                var posX = overviewXAxis.InverseTransform(args.Position.X);

                var xAxis = MainPlotModel.Axes.First(ax => ax.Tag == "X");
                // MainView의 중심을 찾기 위한 Width/2 를 구한다
                var axisHalfWidth = (xAxis.ActualMaximum - xAxis.ActualMinimum) / 2;

                var newMinimum = 0.0;
                // Center 위치를 지정할 수 있음, MainView 전체를 이동할 수 있는 경우
                if (posX - axisHalfWidth >= 0)
                {
                    newMinimum = posX - axisHalfWidth;
                }
                // Width의 절반 위치가 시작점안에 포함되는 경우는 시작을 0으로 설정
                else
                {
                    newMinimum = 0;
                }

                //관심있는 위치로 이동을 하기 위한 delta 값을 계산한다, scale을 적용한다
                var delta = (xAxis.ActualMinimum - newMinimum) * xAxis.Scale;
                xAxis.Pan(delta);

                MainPlotModel.InvalidatePlot(false);
            });

            overviewController.UnbindAll();
            overviewController.BindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Control, overviewTrackAt);
            overviewController.BindMouseDown(OxyMouseButton.Right, OxyModifierKeys.None, PlotCommands.PanAt);
            overviewController.BindMouseWheel(PlotCommands.ZoomWheel);

            return overviewController;
        }

        /// <summary>
        /// X Axis의 AxisChanged 이벤트 동작에 따른 처리를 수행하는 함수
        /// </summary>
        /// <param name="e"></param>
        private void OnMainViewAxisChanged(AxisChangedEventArgs e)
        {
            if (UpdateSubRange == false)
                return;

            var xAxis = MainPlotModel.Axes.FirstOrDefault(a => a.Position == AxisPosition.Bottom);
            if (xAxis != null)
            {
                var newData = sharedService.InterpolatedSamples.SamplesInTimeRange(
                    Instant.FromUnixTimeMilliseconds((long)xAxis.ActualMinimum * 10),
                    Instant.FromUnixTimeMilliseconds((long)xAxis.ActualMaximum * 10)).ConvertToDoubleArray();

                // 현재의 xAxis range에 따라서 step 값을 변경한다
                var axisRange = xAxis.ActualMaximum - xAxis.ActualMinimum;
                if (axisRange < 500)
                {
                    xAxis.MajorStep = 100;
                }
                else if (axisRange < 1000)
                {
                    xAxis.MajorStep = 500;
                }
                else
                {
                    xAxis.MajorStep = 1000;
                }

                var heatMapSeries = MainPlotModel.Series.OfType<HeatMapSeries>().FirstOrDefault();
                if (heatMapSeries != null)
                {
                    heatMapSeries.Data = newData;
                    // LinearAxis 에 의해서 위치가 변경되었으므로, Series 에서도 데이터를 해당 위치에 출력하도록 한다.
                    heatMapSeries.X0 = (int)xAxis.ActualMinimum;
                    heatMapSeries.X1 = (int)xAxis.ActualMaximum;
                }
            }

            // logger.LogTrace("axis changed");
        }

        /// <summary>
        /// Main PlotViw를 위한 별도의 Axes 설정을 한다.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="xSize"></param>
        /// <param name="ySize"></param>
        private void AddAxesOnMain(PlotModel model, int xSize, int ySize)
        {
            model.Axes.Clear();
            model.Axes.Add(new LinearColorAxis
            {
                Position = AxisPosition.Left,
                Palette = SelectedPalette,
                HighColor = SelectedPalette.Colors.Last(),
                LowColor = SelectedPalette.Colors.First(),
                RenderAsImage = false,
                AbsoluteMinimum = MinSensorData,
                AbsoluteMaximum = MaxSensorData,
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
                // X축 데이터가 10msec 단위이므로 100으로 나누면 sec 가 된다.
                LabelFormatter = value => $"{(value / 100).ToString()}",
                Position = AxisPosition.Bottom,
                MinimumPadding = 0,
                Minimum = 0,
                Maximum = plotSampleData.GetLength(0) - 1, // xSize - 1,
                MajorStep = 500,
                MinorStep = 100, // 최대 범위를 입력하여 MinorStep 이 표시되지 않도록 한다
                MajorTickSize = 4,
                MinorTickSize = 2,
                AbsoluteMinimum = 0,
                AbsoluteMaximum = xSize - 1,
                Tag = "X"
            });
        }

        /// <summary>
        /// Main PlotViw를 위한 별도의 Axes 설정을 한다.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="xSize"></param>
        /// <param name="ySize"></param>
        private void AddAxesOnOverview(PlotModel model, int xSize, int ySize)
        {
            model.Axes.Clear();
            model.Axes.Add(new LinearColorAxis
            {
                Position = AxisPosition.None,
                Palette = SelectedPalette,
                HighColor = SelectedPalette.Colors.Last(), // OxyColors.White,
                LowColor = SelectedPalette.Colors.First(),
                Tag = "Color"
            });

            // Y-Axiss
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
                // X축 데이터가 10msec 단위이므로 100으로 나누면 sec 가 된다.
                LabelFormatter = value => $"{(value / 100).ToString()} sec",
                Position = AxisPosition.Bottom,
                MinimumPadding = 0,
                Minimum = 0,
                Maximum = xSize - 1, //100000,// - 1,
                MinorStep = 1000, // 최대 범위를 입력하여 MinorStep 이 표시되지 않도록 한다
                MajorStep = 5000,
                MajorTickSize = 4,
                MinorTickSize = 2,
                AbsoluteMinimum = 0,
                AbsoluteMaximum = xSize - 1,
                Tag = "X"
            });
        }

        [RelayCommand]
        private void ZoomInOut(double zoomVal)
        {
            TimeDuration += (long)zoomVal;
            var mainX = MainPlotModel.Axes.First(ax => (string)ax.Tag == "X");
            mainX.Zoom(mainX.ActualMinimum, mainX.ActualMinimum + TimeDuration / 10);
            MainPlotModel.InvalidatePlot(true);
            // OverviewPlotModel.InvalidatePlot(false);
            logger.LogTrace($"Zoom Timeduration : {TimeDuration / 1000}sec");
        }

        private void ZoomInOutAt(double zoomVal, ScreenPoint pos)
        {
            TimeDuration += (long)zoomVal;
            var mainX = MainPlotModel.Axes.First(ax => (string)ax.Tag == "X");
            var mainY = MainPlotModel.Axes.First(ax => (string)ax.Tag == "Y");
            var xPos = mainX.InverseTransform(pos.X, pos.Y, mainY).X;
            var minX = (xPos - TimeDuration / 10 / 2);
            var maxX = (xPos + TimeDuration / 10 / 2);
            mainX.Zoom(minX, maxX);
            MainPlotModel.InvalidatePlot(true);
            logger.LogTrace($"Zoom Timeduration : {TimeDuration / 1000} sec");
        }

        [RelayCommand(CanExecute = "IsDataLoaded")]
        private void FitToScreen()
        {
            sharedService.TimeFrames.Refresh();
            var overviewX = OverviewPlotModel.Axes.First(ax => (string)ax.Tag == "X");
            overviewX.Zoom(overviewX.AbsoluteMinimum, overviewX.AbsoluteMaximum);
            OverviewPlotModel.InvalidatePlot(false);
        }

        [RelayCommand]
        private void FavoritePalette(FavoritePalette favPalette)
        {
            var name = favPalette.PaletteName;
            MinSensorRange = favPalette.LowerValue;
            MaxSensorRange = favPalette.UpperValue;

            if (Palettes != null && Palettes.ContainsKey(name))
            {
                logger.LogTrace($"Select favorite palette {name}");
                SelectedPaletteKey = name;
            }
            else
            {
                logger.LogError("No registered PaletteKey");
            }
        }

        [RelayCommand]
        private void SensorRangeChanged()
        {
            UpdatePaletteChanged();
        }


        /// <summary>
        /// Palette가 변경시에 OxyPlot 을 업데이트 한다
        /// </summary>
        private void UpdatePaletteChanged()
        {
            if (MainPlotModel.Series.Count == 0)
                return;

            if (!string.IsNullOrEmpty(SelectedPaletteKey) && Palettes.ContainsKey(SelectedPaletteKey))
                SelectedPalette = Palettes[SelectedPaletteKey];

            paletteManager.SetPaletteKey(SelectedPaletteKey);
            // TimeFrameViewModel.SelectedPalette = SelectedPalette;

            var mainColorAxis = MainPlotModel.Axes.Single(s => s is LinearColorAxis) as LinearColorAxis;
            mainColorAxis.Palette = SelectedPalette;
            mainColorAxis.Minimum = MinSensorRange; // 최소 limit 값
            mainColorAxis.Maximum = MaxSensorRange; // 최대 limit 값
            mainColorAxis.HighColor = SelectedPalette.Colors.Last(); // OxyColors.White,
            mainColorAxis.LowColor = SelectedPalette.Colors.First();
            MainPlotModel.InvalidatePlot(false);

            var overviewColorAxis = OverviewPlotModel.Axes.Single(s => s is LinearColorAxis) as LinearColorAxis;
            overviewColorAxis.Palette = SelectedPalette;
            overviewColorAxis.Minimum = MinSensorRange; // 최소 limit 값
            overviewColorAxis.Maximum = MaxSensorRange; // 최대 limit 값
            overviewColorAxis.HighColor = SelectedPalette.Colors.Last(); // OxyColors.White,
            overviewColorAxis.LowColor = SelectedPalette.Colors.First();
            OverviewPlotModel.InvalidatePlot(false);

            var changedArg = new PaletteChangedMessageArg()
            {
                palette = SelectedPalette,
                Minimum = MinSensorRange,
                Maximum = MaxSensorRange,
                HighColor = SelectedPalette.Colors.Last(),
                LowColor = SelectedPalette.Colors.First(),
            };

            //Pallette 가 변경되었음을 메시지로 전송한다.
            WeakReferenceMessenger.Default.Send(new PaletteChangedMessageMessage(changedArg));
        }

        /// <summary>
        /// Explorer Page로 이동을 요청한다.
        /// </summary>
        [RelayCommand]
        private void NavigateToExplorer()
        {
            logger.LogTrace($"Request navigate to Explorer view");
            WeakReferenceMessenger.Default.Send(new TabIndexChangeMessage(1));
        }

        /// <summary>
        /// MainView의 현재 위치에서 가장 가까운 이전 FrameNote를 화면 중앙으로 표시되도록 한다.
        /// </summary>
        [RelayCommand]
        private void PrevFrameNote()
        {
            var xAxis = MainPlotModel.Axes.First(ax => ax.Tag == "X");
            var axisHalfWidth = (xAxis.ActualMaximum - xAxis.ActualMinimum) / 2;
            try
            {
                var posX = (double)timeFrames.KeyValues.Where(kv =>
                        (double)kv.Value.Time.ToUnixTimeMilliseconds() / 10 < (xAxis.ActualMinimum + axisHalfWidth) - 1)
                    .Select(kv => kv.Value.Time.ToUnixTimeMilliseconds()).Last() / 10;

                var newMinimum = 0.0;
                // Center 위치를 지정할 수 있음
                if (posX - axisHalfWidth >= 0)
                {
                    newMinimum = posX - axisHalfWidth;
                }
                else
                {
                    newMinimum = 0;
                }

                var delta = (xAxis.ActualMinimum - newMinimum) * xAxis.Scale;
                xAxis.Pan(delta);
                MainPlotModel.InvalidatePlot(false);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 
        /// MainView의 현재 위치에서 가장 가까운 다음 FrameNote를 화면 중앙으로 표시되도록 한다.
        /// </summary>
        [RelayCommand]
        private void NextFrameNote()
        {
            var xAxis = MainPlotModel.Axes.First(ax => ax.Tag == "X");
            var axisHalfWidth = (xAxis.ActualMaximum - xAxis.ActualMinimum) / 2;

            try
            {
                var posX = (double)timeFrames.KeyValues.Where(kv =>
                        kv.Value.Time.ToUnixTimeMilliseconds() / 10 > (xAxis.ActualMinimum + axisHalfWidth) + 1)
                    .Select(kv => kv.Value.Time.ToUnixTimeMilliseconds()).First() / 10;

                var newMinimum = 0.0;
                // Center 위치를 지정할 수 있음
                if (posX - axisHalfWidth >= 0)
                {
                    newMinimum = posX - axisHalfWidth;
                }
                else
                {
                    newMinimum = 0;
                }

                var delta = (xAxis.ActualMinimum - newMinimum) * xAxis.Scale;
                xAxis.Pan(delta);
                MainPlotModel.InvalidatePlot(false);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Sensor Bounds의 upper/lower 값을 변경할 때 표시할 높이를 변경한다
        /// </summary>
        [RelayCommand]
        private void SensorBoundsChanged()
        {
            var minPos = MinSensorBound / (ExamSensorSize) * (MainPlotModel.PlotArea.Height - 8);
            var maxPos = (ExamSensorSize - MaxSensorBound) * (MainPlotModel.PlotArea.Height - 8) / ExamSensorSize;
            SensorBoundLowerHeight = minPos;
            SensorBoundUpperHeight = maxPos;
        }

        /// <summary>
        /// SensorBound edit 상태가 변경되었을 때에 이를 mainplotview 에 적용하도록 한다
        /// </summary>
        /// <param name="sender"></param>
        [RelayCommand]
        private void ToggleSensorBounds(bool sender)
        {
            PickingSensorBounds = sender;
            var heatMapSeries = MainPlotModel.Series.OfType<HeatMapSeries>().FirstOrDefault();
            var mainYAxis = MainPlotModel.Axes.First(ax => ax.Tag == "Y");

            // 전체 영역을 표시하도록 한다
            if (PickingSensorBounds)
            {
                if (heatMapSeries != null)
                {
                    // LinearAxis 에 의해서 위치가 변경되었으므로, Series 에서도 데이터를 해당 위치에 출력하도록 한다.
                    // 전체 영역을 표시하도록 한다
                    mainYAxis.Minimum = mainYAxis.AbsoluteMinimum;
                    mainYAxis.Maximum = mainYAxis.AbsoluteMaximum;

                    MainPlotModel.InvalidatePlot(true);
                }

                var area = MainPlotModel.PlotArea;

                // View에서 그려지는 SensorBound 의 Margin 을 설정한다
                SensorBoundUpperMargin = new Thickness(area.Left, area.Top, 0, 0);
                SensorBoundLowerMargin = new Thickness(area.Left, 0, 0, MainPlotModel.Height - area.Bottom);
                SensorBoundsSliderMargin = new Thickness(area.Left - 12, area.Top - 4, 0, 0);

                SensorBoundHeight = MainPlotModel.PlotArea.Height + 8;
                SensorBoundWidth = area.Width;
            }
            // Bounds 를 설정한 sensor에 대해서만 표시하도록 한다
            else
            {
                if (heatMapSeries != null)
                {
                    // LinearAxis 에 의해서 위치가 변경되었으므로, Series 에서도 데이터를 해당 위치에 출력하도록 한다.
                    var sensorIntpScale = (mainYAxis.ActualMaximum + 1) / ExamSensorSize;
                    mainYAxis.Minimum = MinSensorBound * sensorIntpScale;
                    mainYAxis.Maximum = MaxSensorBound * sensorIntpScale - 1;

                    MainPlotModel.InvalidatePlot(true);

                    WeakReferenceMessenger.Default.Send(new SensorBoundsChangedMessage(
                        new SensorBoundsChangedMessageArg() { MinBound = MinSensorBound, MaxBound = MaxSensorBound }));
                }
            }
        }

        

        /// <summary>
        /// WindowSize가 변경되면 SensorBounds 관련 control을 업데이트 한다.
        /// </summary>
        /// <param name="sender"></param>
        [RelayCommand]
        private void WindowSizeChanged(object sender)
        {
            if (PickingSensorBounds)
            {
                ToggleSensorBounds(true);
                SensorBoundsChanged();
            }
        }
    }
}