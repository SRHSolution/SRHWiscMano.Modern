﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoreLinq.Extensions;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
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

            sharedService.ExamDataLoaded += SharedService_ExamDataLoaded;

            Palettes = paletteManager.Palettes; //PaletteUtils.GetPredefinedPalettes();

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

        private void SharedService_ExamDataLoaded(object? sender, EventArgs e)
        {
            LoadExamDataImpl();
        }

        private void LoadExamDataImpl()
        {
            var examData = sharedService.ExamData;
            var sensorCount = (int)(examData.SensorCount() * InterpolateSensorScale);
            var frameCount = examData.Samples.Count;

            fullExamData = examData.PlotData;

            // 입력받은 Exam 데이터에서 최소 최대 값을 얻어 RangeSlider의 최소/최대 값을 변경한다
            MinSensorData = Math.Floor(fullExamData.Cast<double>().Min());
            MaxSensorData = Math.Ceiling(fullExamData.Cast<double>().Max());

            // 기존의 PlotView를 clear 한 후 ExamData에 대한 PlotModel을 생성해서 입력한다.
            ((IPlotModel)this.MainPlotModel)?.AttachPlotView(null);

            //Mainview plotmodel, controller 설정
            var mainModel = new PlotModel();
            PlotDataUtils.AddHeatmapSeries(mainModel, fullExamData);
            AddAxesOnMain(mainModel, frameCount, sensorCount);
            // AddFrameNotes(mainModel, examData.Notes.t);
            if (UpdateSubRange)
            {
                var heatMapSeries = mainModel.Series.OfType<HeatMapSeries>().FirstOrDefault();
                heatMapSeries.Data = PlotDataUtils.CreateSubRange(fullExamData, 0, settings.MainViewFrameRange - 1, 0, sensorCount-1);
                heatMapSeries.X1 = settings.MainViewFrameRange;
            }

            // SubRange 업데이트 기능을 위한 이벤트 등록
            mainModel.Axes.First(ax=>ax.Tag == "X").AxisChanged += OnMainViewAxisChanged;
            MainPlotModel = mainModel;

            var mainController = new PlotController();
            var lineAnnotPan = new DelegatePlotCommand<OxyMouseDownEventArgs>((view, controller, args) => controller.AddMouseManipulator(view, new LineAnnotationManipulator(view)
            {
                IsVertical = true,
            }, args));

            mainController.BindMouseDown(OxyMouseButton.Left, lineAnnotPan);
            MainPlotController = mainController;


            //Overview plotmodel, controller 설정

            ((IPlotModel)this.OverviewPlotModel)?.AttachPlotView(null);
            var overviewModel = CreatePlotModel((double[,])fullExamData.Clone());
            AddAxesOnOverview(overviewModel, frameCount, sensorCount);
            // AddFrameNotes(overviewModel, examData.Notes.ToList());
            OverviewPlotModel = overviewModel;

            var overviewController = new PlotController();
            var overviewTrackAt = new DelegatePlotCommand<OxyMouseDownEventArgs>((view, controller, args) =>
            {
                var overviewXAxis = view.ActualModel.Axes.First(ax => ax.Tag == "X");
                var posX = overviewXAxis.InverseTransform(args.Position.X);

                var xAxis = MainPlotModel.Axes.First(ax => ax.Tag == "X");
                var axisHalfWidth = (xAxis.ActualMaximum - xAxis.ActualMinimum)/2;

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

                var delta = (xAxis.ActualMinimum- newMinimum) * xAxis.Scale;
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

        private void OnMainViewAxisChanged(object? sender, AxisChangedEventArgs e)
        {
            if (UpdateSubRange == false)
                return;

            var xAxis = MainPlotModel.Axes.FirstOrDefault(a => a.Position == AxisPosition.Bottom);
            if (xAxis != null )
            {
                double[,] newData = PlotDataUtils.CreateSubRange(fullExamData, (int)xAxis.ActualMinimum, (int)xAxis.ActualMaximum, 0, fullExamData.GetLength(1) - 1);

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

        [RelayCommand(CanExecute = "IsDataLoaded")]
        private void FitToScreen()
        {
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
            
            if(!string.IsNullOrEmpty(SelectedPaletteKey) && Palettes.ContainsKey(SelectedPaletteKey))
                SelectedPalette = Palettes[SelectedPaletteKey];

            paletteManager.SetPaletteKey(SelectedPaletteKey);
            TimeFrameViewModel.SelectedPalette = SelectedPalette;


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