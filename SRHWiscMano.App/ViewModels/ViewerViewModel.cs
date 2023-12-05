using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoreLinq.Extensions;
using NLog;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using SRHWiscMano.App.Data;
using SRHWiscMano.App.Services;
using SRHWiscMano.Core.Control;
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

        #endregion

        public IExamination ExamData { get; private set; }
        public ObservableCollection<FrameNote> Notes { get; }

        [ObservableProperty] private bool imageVisibility;

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

        private bool updateSubRange = false;
        public bool UpdateSubRange
        {
            get => updateSubRange;
            set
            {
                SetProperty(ref updateSubRange, value);
                if (MainPlotModel != null && value == false)
                {
                    var heatMapSeries = MainPlotModel.Series.OfType<HeatMapSeries>().FirstOrDefault();
                    var xAxis = MainPlotModel.Axes.FirstOrDefault(a => a.Position == AxisPosition.Bottom);
                    if (heatMapSeries != null)
                    {
                        heatMapSeries.Data = fullExamData;
                        heatMapSeries.X0 = 0;
                        heatMapSeries.X1 = fullExamData.GetLength(0)-1;
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

        public void AxisFreeze()
        {
            var xAxis = MainPlotModel.Axes.First(ax => ax.Tag == "X");
            var minimum = xAxis.ActualMinimum;
            var maximum = xAxis.ActualMaximum;
            xAxis.AbsoluteMinimum = minimum;
            xAxis.AbsoluteMaximum = maximum;
        }

        public void AxisRelease()
        {
            var series = MainPlotModel.Series[0] as HeatMapSeries;
            
            var xAxis = MainPlotModel.Axes.First(ax => ax.Tag == "X");
            var minimum = series.MinX;
            var maximum = series.MaxX;

            xAxis.AbsoluteMinimum = minimum;
            xAxis.AbsoluteMaximum = maximum;
        }

        public IRelayCommand FitToScreenCommand { get; }
        public IRelayCommand PrevFrameNoteCommand { get; private set; }
        public IRelayCommand NextFrameNoteCommand { get; private set; }

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

        public ViewerViewModel(ILogger<ViewerViewModel> logger, SharedService sharedService,
            IOptions<AppSettings> settings)
        {
            this.logger = logger;
            this.sharedService = sharedService;
            sharedService.ExamDataLoaded += SharedService_ExamDataLoaded;

            Palettes = PaletteUtils.GetPredefinedPalettes();

            MaxSensorData = 100;
            MinSensorData = -10;

            MainPlotModel = new PlotModel();
            OverviewPlotModel = new PlotModel();

            pvBackColor = settings.Value.BaseBackColor;
            pvForeColor = settings.Value.BaseForeColor;
            // ApplyThemeToOxyPlots();

            FitToScreenCommand = new RelayCommand(FitToScreen, ()=> IsDataLoaded);
            
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
            var backColor = pvBackColor;
            var foreColor = pvForeColor;

            MainPlotModel.Background = OxyColor.FromArgb(backColor.A, backColor.R, backColor.G, backColor.B);
            MainPlotModel.TextColor = OxyColor.FromArgb(foreColor.A, foreColor.R, foreColor.G, foreColor.B);
            MainPlotModel.PlotAreaBorderColor = OxyColors.Gray;
            MainPlotModel.Axes.Where(ax => ax.GetType().Name == "LinearAxis").ForEach(lax =>
            {
                lax.TicklineColor = MainPlotModel.TextColor;
                lax.MinorTicklineColor = MainPlotModel.TextColor;
            });
            MainPlotModel.InvalidatePlot(false);

            OverviewPlotModel.Background = OxyColor.FromArgb(backColor.A, backColor.R, backColor.G, backColor.B);
            OverviewPlotModel.TextColor = OxyColor.FromArgb(foreColor.A, foreColor.R, foreColor.G, foreColor.B);
            OverviewPlotModel.PlotAreaBorderColor = OxyColors.Gray;
            OverviewPlotModel.Axes.Where(ax => ax.GetType().Name == "LinearAxis").ForEach(lax =>
            {
                lax.TicklineColor = OverviewPlotModel.TextColor;
                lax.MinorTicklineColor = OverviewPlotModel.TextColor;
            });
            OverviewPlotModel.InvalidatePlot(false);
        }

        private HeatMapSeries backupHeatmap;
        private double[,] fullExamData;

        private void SharedService_ExamDataLoaded(object? sender, EventArgs e)
        {
            LoadExamDataImpl();
        }

        private async Task LoadExamDataImpl()
        {
            var examData = sharedService.ExamData;
            var sensorCount = (int)(examData.SensorCount() * InterpolateSensorScale);
            var frameCount = examData.Samples.Count;
            fullExamData = new double[frameCount, sensorCount];
            await Task.Run(() =>
            {
                for (int i = 0; i < frameCount; i++)
                {
                    var scaledSensorValues =
                        Interpolators.LinearInterpolate(examData.Samples[i].Values.ToArray(), sensorCount);

                    for (int j = 0; j < sensorCount; j++)
                    {
                        fullExamData[i, j] = scaledSensorValues[sensorCount - 1 - j]; 
                    }
                }

                logger.LogInformation($"ExamData's sensor data is interpolated by {InterpolateSensorScale} times");

                // 입력받은 Exam 데이터에서 최소 최대 값을 얻어 RangeSlider의 최소/최대 값을 변경한다
                MinSensorData = Math.Floor(fullExamData.Cast<double>().Min());
                MaxSensorData = Math.Ceiling(fullExamData.Cast<double>().Max());

                logger.LogInformation($"ExamData has min/max : {MinSensorData}/{MaxSensorData}");
            });


            // 기존의 PlotView를 clear 한 후 ExamData에 대한 PlotModel을 생성해서 입력한다.
            ((IPlotModel)this.MainPlotModel)?.AttachPlotView(null);

            double[,] mainData = fullExamData;
            // mainData = CreateSubRange(fullExamData, 0, 2000, 0, fullExamData.GetLength(1) - 1);

            var mainModel = CreatePlotModel(mainData);
            backupHeatmap = (HeatMapSeries)mainModel.Series[0];
            AddAxesOnMain(mainModel, frameCount, sensorCount);
            AddFrameNotes(mainModel, examData.Notes.ToList());

            MainPlotModel = mainModel;
            MainPlotModel.Axes.First(ax=>ax.Tag == "X").AxisChanged += OnAxisChanged;

            var mainController = new PlotController();
            var lineAnnotPan = new DelegatePlotCommand<OxyMouseDownEventArgs>((view, controller, args) => controller.AddMouseManipulator(view, new LineAnnotationManipulator(view)
            {
                IsVertical = true,
            }, args));

            mainController.BindMouseDown(OxyMouseButton.Left, lineAnnotPan);

            // mainController.BindMouseEnter(OxyPlot.PlotCommands.Tr);
            MainPlotController = mainController;

            ((IPlotModel)this.OverviewPlotModel)?.AttachPlotView(null);
            var overviewModel = CreatePlotModel((double[,])fullExamData.Clone());
            AddAxesOnOverview(overviewModel, frameCount, sensorCount);
            AddFrameNotes(overviewModel, examData.Notes.ToList());
            OverviewPlotModel = overviewModel;



            ApplyThemeToOxyPlots();

            IsDataLoaded = true;
        }    

        private void OnAxisChanged(object? sender, AxisChangedEventArgs e)
        {
            if (UpdateSubRange == false)
                return;

            var xAxis = MainPlotModel.Axes.FirstOrDefault(a => a.Position == AxisPosition.Bottom);
            if (xAxis != null )
            {
                double[,] newData = CreateSubRange(fullExamData, (int)xAxis.ActualMinimum, (int)xAxis.ActualMaximum, 0, fullExamData.GetLength(1) - 1);

                var heatMapSeries = MainPlotModel.Series.OfType<HeatMapSeries>().FirstOrDefault();
                if (heatMapSeries != null)
                {
                    heatMapSeries.Data = newData;
                    // LinearAxis 에 의해서 위치가 변경되었으므로, Series 에서도 데이터를 해당 위치에 출력하도록 한다.
                    heatMapSeries.X0 = (int)xAxis.ActualMinimum;
                    heatMapSeries.X1 = (int)xAxis.ActualMaximum;
                }
            }
            logger.LogTrace("axis changed");
        }

        public double[,] CreateSubRange(double[,] originalArray, int startRow, int endRow, int startColumn, int endColumn)
        {
            int numRows = endRow - startRow + 1;
            int numCols = endColumn - startColumn + 1;
            double[,] subArray = new double[numRows, numCols];

            for (int i = startRow; i <= endRow; i++)
            {
                for (int j = startColumn; j <= endColumn; j++)
                {
                    subArray[i - startRow, j - startColumn] = originalArray[i, j];
                    // logger.LogDebug($"subrange {i}, {j}");
                }
            }

            return subArray;
        }


        /// <summary>
        /// 공통 데이터를 이용하므로 Main, Overview에 대한 PlotModel을 생성한다.
        /// </summary>
        /// <param name="examData"></param>
        /// <param name="plotData"></param>
        /// <returns></returns>
        private PlotModel CreatePlotModel(double[,] plotData)
        {
            var frameCount = plotData.GetLength(0);
            var sensorCount = plotData.GetLength(1);
            var model = new PlotModel {Title = ""};

            // Create your heatmap series and add to MyModel
            var heatmapSeries = new HeatMapSeries
            {
                CoordinateDefinition = HeatMapCoordinateDefinition.Center,
                X0 = 0,
                X1 = (double)frameCount, 
                Y0 = 0,
                Y1 = sensorCount, //plotData.GetLength(1),
                Data = plotData /* Your 2D data array */,
                Interpolate = true,
                RenderMethod = HeatMapRenderMethod.Bitmap,
                Tag = "Heatmap"
            };

            model.Series.Add(heatmapSeries);

            return model;
        }


        /// <summary>
        /// Main PlotViw를 위한 별도의 Axes 설정을 한다.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="xSize"></param>
        /// <param name="ySize"></param>
        private void AddAxesOnMain(PlotModel model, int xSize, int ySize)
        {
            foreach (var axis in model.Axes)
            {
                model.Axes.Remove(axis);
            }

            model.Axes.Add(new LinearColorAxis
            {
                Position = AxisPosition.Left,
                Palette = SelectedPalette,
                HighColor = SelectedPalette.Colors.Last(), // OxyColors.White,
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
                LabelFormatter = value=> $"{(value/1000).ToString()}",
                Position = AxisPosition.Bottom,
                MinimumPadding = 0,
                Minimum = 0,
                Maximum = 2000 , // - 1,
                MajorStep = 1000,
                // MinorStep = xSize - 1, // 최대 범위를 입력하여 MinorStep 이 표시되지 않도록 한다
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
            foreach (var axis in model.Axes)
            {
                model.Axes.Remove(axis);
            }

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
                MinorStep = 1000 , // 최대 범위를 입력하여 MinorStep 이 표시되지 않도록 한다
                MajorStep = 5000 ,
                MajorTickSize = 4,
                MinorTickSize = 2,
                AbsoluteMinimum = 0,
                AbsoluteMaximum = xSize - 1,
                Tag = "X"
            });
        }

        private void AddFrameNotes(PlotModel model, List<FrameNote> notes)
        {
            foreach (var note in notes)
            {
                var msec = note.Time.ToMillisecondsFromEpoch() / 10;
                AnnoationUtils.CreateVLineAnnotation(msec, note.Text, false, model);
            }
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

        private void FitToScreen()
        {
            if (MainPlotModel.Series.Count > 0 && MainPlotModel.Series[0].IsVisible)
            {
                MainPlotModel.Series[0].IsVisible = false;
                // MainPlotModel.Series.RemoveAt(0);
            }
            else
            {
                // MainPlotModel.Series.Add(backupHeatmap);
                MainPlotModel.Series[0].IsVisible = true;
            }
            MainPlotModel.InvalidatePlot(false);
            

            return;


            var mainX = MainPlotModel.Axes.First(ax => (string)ax.Tag == "X");
            mainX.Minimum = 0;
            mainX.Maximum = mainX.AbsoluteMaximum;
            mainX.Reset();

            var overviewX = OverviewPlotModel.Axes.First(ax => (string)ax.Tag == "X");
            overviewX.Minimum = 0;
            overviewX.Maximum = overviewX.AbsoluteMaximum;
            overviewX.Reset();

            // MainPlotModel.InvalidatePlot(false);
            // OverviewPlotModel.InvalidatePlot(false);
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
                var customColors = new OxyColor[]
                {
                    OxyColor.FromArgb(255, 24, 3, 95),
                    OxyColor.FromArgb(255, 30, 237, 215),
                    OxyColor.FromArgb(255, 47, 243, 38),
                    OxyColor.FromArgb(255, 248, 248, 1),
                    OxyColor.FromArgb(255, 253, 5, 0),
                    OxyColor.FromArgb(255, 95, 0, 69)
                };
                var colorCount = favPalette.UpperValue - favPalette.LowerValue;
                var palette = OxyPalette.Interpolate(colorCount, customColors);

                logger.LogTrace($"Select custom palette");
                SelectedPalette = palette;
                SelectedPaletteKey = "Custom";
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
            
            var mainColorAxis = MainPlotModel.Axes.Single(s => s is LinearColorAxis) as LinearColorAxis;
            if(!string.IsNullOrEmpty(SelectedPaletteKey) && Palettes.ContainsKey(SelectedPaletteKey))
                SelectedPalette = Palettes[SelectedPaletteKey];
            
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
    }
}