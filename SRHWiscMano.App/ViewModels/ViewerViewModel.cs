using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Reactive.Linq;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using System.Windows.Controls;
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

        [ObservableProperty] private double minSensorData;
        [ObservableProperty] private double maxSensorData;
        [ObservableProperty] private double minSensorRange = 0;
        [ObservableProperty] private double maxSensorRange = 100;
        [ObservableProperty] private double zoomPercentage = 100;
        [ObservableProperty] private OxyPalette selectedPalette = OxyPalettes.Hue64;
        [ObservableProperty] private double interpolateSensorScale = 10;

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

        private double[,] fullExamData;
        private IDisposable axisChangeObserver = null;
        private readonly SourceCache<ITimeFrame, int> timeFrames;


        private void HandleTimeFrames(IChangeSet<ITimeFrame, int> changeSet)
        {
            foreach (var change in changeSet)
            {
                switch (change.Reason)
                {
                    case ChangeReason.Add:
                    {
                        var item = change.Current;
                        var msec = item.Time.ToMillisecondsFromEpoch() / 10;
                        CreateVLineAnnotation(item, true, MainPlotModel);
                        CreateVLineAnnotation(item, false, OverviewPlotModel);
                        break;
                    }
                    case ChangeReason.Remove:
                    {
                        var mainAnno = MainPlotModel.Annotations.OfType<LineAnnotation>().Single(item => (int) item.Tag == change.Current.Id);
                        MainPlotModel.Annotations.Remove(mainAnno);

                        var overAnno = OverviewPlotModel.Annotations.OfType<LineAnnotation>().Single(item => (int) item.Tag == change.Current.Id);
                        OverviewPlotModel.Annotations.Remove(overAnno);

                        logger.LogTrace($"Removed: {change.Current.Text}");
                        break;
                    }
                    case ChangeReason.Moved:
                        break;
                    case ChangeReason.Update:
                    {
                        var msec = change.Current.Time.ToMillisecondsFromEpoch() / 10;
                        var mainAnno = MainPlotModel.Annotations.OfType<LineAnnotation>().Single(item => (int) item.Tag == change.Current.Id);
                        mainAnno.Text = change.Current.Text;
                        mainAnno.X = msec;
                        MainPlotModel.InvalidatePlot(false);

                        var overAnno = OverviewPlotModel.Annotations.OfType<LineAnnotation>().Single(item => (int) item.Tag == change.Current.Id);
                        overAnno.Text = change.Current.Text;
                        overAnno.X = msec;
                        OverviewPlotModel.InvalidatePlot(false);

                        logger.LogTrace($"Update: {change.Current.Text}");
                        break;
                    }
                    case ChangeReason.Refresh:
                        break;
                }
            }
        }


        private void CreateVLineAnnotation(ITimeFrame timeFrame, bool draggable, PlotModel model)
        {
            var msec = timeFrame.Time.ToMillisecondsFromEpoch() / 10;
            var la = new LineAnnotation
            {
                Type = LineAnnotationType.Vertical,
                X = msec,
                LineStyle = LineStyle.Solid,
                ClipByYAxis = true,
                Text = timeFrame.Text,
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
                    la.X = (long) la.InverseTransform(e.Position).X;
                    model.InvalidatePlot(false);
                    e.Handled = true;
                };

                la.MouseUp += (s, e) =>
                {
                    la.StrokeThickness /= 5;
                    model.InvalidatePlot(false);

                    la.X = (long) la.InverseTransform(e.Position).X;
                    timeFrame.UpdateTime(Instant.FromUnixTimeMilliseconds((long) la.X * 10));
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

        private void LoadExamDataImpl()
        {
            var examData = sharedService.ExamData;
            var sensorCount = (int) (examData.SensorCount() * InterpolateSensorScale);
            var frameCount = examData.Samples.Count;

            fullExamData = examData.PlotData;

            // 입력받은 Exam 데이터에서 최소 최대 값을 얻어 RangeSlider의 최소/최대 값을 변경한다
            MinSensorData = Math.Floor(fullExamData.Cast<double>().Min());
            MaxSensorData = Math.Ceiling(fullExamData.Cast<double>().Max());

            // 기존의 PlotView를 clear 한 후 ExamData에 대한 PlotModel을 생성해서 입력한다.
            // ((IPlotModel)this.MainPlotModel)?.AttachPlotView(null);

            //Mainview plotmodel, controller 설정
            var mainModel = MainPlotModel;
            var examPlotData = fullExamData;
            // AddFrameNotes(mainModel, examData.Notes.t);
            if (UpdateSubRange)
            {
                examPlotData = PlotDataUtils.CreateSubRange(fullExamData, 0, settings.MainViewFrameRange - 1, 0,
                    sensorCount - 1);
            }

            PlotDataUtils.AddHeatmapSeries(mainModel, examPlotData);

            AddAxesOnMain(mainModel, frameCount, sensorCount);

            // SubRange 업데이트 기능을 위한 이벤트 등록
            var xAxis = mainModel.Axes.First(ax => ax.Tag == "X");
            if (axisChangeObserver != null)
                axisChangeObserver.Dispose();

            axisChangeObserver = Observable.FromEvent<EventHandler<AxisChangedEventArgs>, AxisChangedEventArgs>(
                handler => (sender, e) => handler(e),
                handler => xAxis.AxisChanged += handler,
                handler => xAxis.AxisChanged -= handler).Subscribe(OnMainViewAxisChanged);

            var mainController = new PlotController();

            // Bind 하게 되면 기존의 MouseWheel(zoom) 이 삭제된다.
            mainController.BindMouseWheel(new DelegatePlotCommand<OxyMouseWheelEventArgs>((v, c, a) =>
            {
                var m = new ZoomStepManipulator(v) {Step = a.Delta * 0.001 * 1, FineControl = a.IsControlDown};
                m.Started(a);
                var range = xAxis.ActualMaximum - xAxis.ActualMinimum + 1;
                settings.MainViewFrameRange = (int) range;
            }));

            // LineAnnotation을 Panning 하는 MouseManipulator Command를 추가한다.
            // controller에 추가한 command가 완료(mouse up)되면 제거된다. 
            var lineAnnotPan = new DelegatePlotCommand<OxyMouseDownEventArgs>((view, controller, args) =>
                controller.AddMouseManipulator(view, new LineAnnotationManipulator(view)
                {
                    IsVertical = true,
                }, args));
            mainController.BindMouseDown(OxyMouseButton.Left, lineAnnotPan);
            MainPlotController = mainController;

            //Overview plotmodel, controller 설정
            // ((IPlotModel)this.OverviewPlotModel)?.AttachPlotView(null);
            var overviewModel = OverviewPlotModel;
            PlotDataUtils.AddHeatmapSeries(overviewModel, fullExamData);
            AddAxesOnOverview(overviewModel, frameCount, sensorCount);
            // AddFrameNotes(overviewModel, examData.Notes.ToList());
            OverviewPlotModel = overviewModel;

            var overviewController = new PlotController();
            var overviewTrackAt = new DelegatePlotCommand<OxyMouseDownEventArgs>((view, controller, args) =>
            {
                var overviewXAxis = view.ActualModel.Axes.First(ax => ax.Tag == "X");
                var posX = overviewXAxis.InverseTransform(args.Position.X);

                var xAxis = MainPlotModel.Axes.First(ax => ax.Tag == "X");
                var axisHalfWidth = (xAxis.ActualMaximum - xAxis.ActualMinimum) / 2;

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
            });
            overviewController.UnbindAll();
            overviewController.BindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Control, overviewTrackAt);
            overviewController.BindMouseDown(OxyMouseButton.Right, OxyModifierKeys.None, PlotCommands.PanAt);
            overviewController.BindMouseWheel(PlotCommands.ZoomWheel);

            OverviewPlotController = overviewController;

            ApplyThemeToOxyPlots();

            SelectedPaletteKey = paletteManager.SelectedPaletteKey;

            IsDataLoaded = true;
        }

        private void OnMainViewAxisChanged(AxisChangedEventArgs e)
        {
            if (UpdateSubRange == false)
                return;

            var xAxis = MainPlotModel.Axes.FirstOrDefault(a => a.Position == AxisPosition.Bottom);
            if (xAxis != null)
            {
                double[,] newData = PlotDataUtils.CreateSubRange(fullExamData, (int) xAxis.ActualMinimum,
                    (int) xAxis.ActualMaximum, 0, fullExamData.GetLength(1) - 1);

                var heatMapSeries = MainPlotModel.Series.OfType<HeatMapSeries>().FirstOrDefault();
                if (heatMapSeries != null)
                {
                    heatMapSeries.Data = newData;
                    // LinearAxis 에 의해서 위치가 변경되었으므로, Series 에서도 데이터를 해당 위치에 출력하도록 한다.
                    heatMapSeries.X0 = (int) xAxis.ActualMinimum;
                    heatMapSeries.X1 = (int) xAxis.ActualMaximum;
                }
            }

            logger.LogTrace("axis changed");
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
                LabelFormatter = value => $"{(value / 1000).ToString()}",
                Position = AxisPosition.Bottom,
                MinimumPadding = 0,
                Minimum = 0,
                Maximum = settings.MainViewFrameRange - 1, // xSize - 1,
                MajorStep = 1000,
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
                AbsoluteMaximum = ySize * InterpolateSensorScale - 1, // Panning 최대값
                IsAxisVisible = false,
                Tag = "Y"
            });

            // X-Axis
            model.Axes.Add(new LinearAxis()
            {
                LabelFormatter = value => $"{(value / 1000).ToString()} sec",
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
        private void ZoomOut(double zoomVal)
        {
            logger.LogTrace($"Zoom : {zoomVal}");
            ZoomPercentage = (int) (ZoomPercentage / zoomVal);
        }

        [RelayCommand]
        private void ZoomIn(double zoomVal)
        {
            logger.LogTrace($"Zoom : {zoomVal}");
            ZoomPercentage = (int) (ZoomPercentage * zoomVal);
        }

        [RelayCommand(CanExecute = "IsDataLoaded")]
        private void FitToScreen()
        {
            sharedService.TimeFrames.Refresh();
            var overviewX = OverviewPlotModel.Axes.First(ax => (string) ax.Tag == "X");
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
        }

        /// <summary>
        /// 
        /// MainView의 현재 위치에서 가장 가까운 다음 FrameNote를 화면 중앙으로 표시되도록 한다.
        /// </summary>
        [RelayCommand]
        private void NextFrameNote()
        {
        }
    }
}