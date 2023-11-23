﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoreLinq.Extensions;
using NLog;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using SRHWiscMano.App.Data;
using SRHWiscMano.App.Services;
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

        private void SharedService_ExamDataLoaded(object? sender, EventArgs e)
        {
            var examData = sharedService.ExamData;
            var sensorCount = (int)(examData.SensorCount() * InterpolateSensorScale);
            var frameCount = examData.Samples.Count;

            var arrayData = new double[frameCount, sensorCount];
            for (int i = 0; i < frameCount; i++)
            {
                var scaledSensorValues = Interpolators.LinearInterpolate(examData.Samples[i].Values.ToArray(), sensorCount);
                
                for (int j = 0; j < sensorCount; j++)
                {
                    arrayData[i, j] = scaledSensorValues[sensorCount - 1 - j];// examData.Samples[i].Values[sensorCount - 1 - j];
                }
            }

            logger.LogInformation($"ExamData's sensor data is interpolated by {InterpolateSensorScale} times");

            // 입력받은 Exam 데이터에서 최소 최대 값을 얻어 RangeSlider의 최소/최대 값을 변경한다
            MinSensorData = Math.Floor(arrayData.Cast<double>().Min());
            MaxSensorData = Math.Ceiling(arrayData.Cast<double>().Max());

            logger.LogInformation($"ExamData has min/max : {MinSensorData}/{MaxSensorData}");

            // 기존의 PlotView를 clear 한 후 ExamData에 대한 PlotModel을 생성해서 입력한다.
            ((IPlotModel) this.MainPlotModel)?.AttachPlotView(null);


            var mainData = CreateSubRange(arrayData, 0, 2000, 0, arrayData.GetLength(1)-1);
            var mainModel = CreatePlotModel(mainData);
            backupHeatmap = (HeatMapSeries)mainModel.Series[0];
            AddAxesOnMain(mainModel, frameCount, sensorCount);
            MainPlotModel = mainModel;

            // var mainController = new PlotController();
            // mainController.BindMouseEnter(OxyPlot.PlotCommands.Tr);
            // MainPlotController = mainController;

            ((IPlotModel) this.OverviewPlotModel)?.AttachPlotView(null);
            var overviewModel = CreatePlotModel((double[,])arrayData.Clone());
            AddAxesOnOverview(overviewModel, frameCount, sensorCount);
            OverviewPlotModel = overviewModel;

            ApplyThemeToOxyPlots();

            IsDataLoaded = true;
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
            if (Palettes != null && Palettes.ContainsKey(name))
            {
                logger.LogTrace($"Select favorite palette {name}");

                MinSensorRange = favPalette.LowerValue;
                MaxSensorRange = favPalette.UpperValue;
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
                SelectedPalette = palette;

                var mainColorAxis = MainPlotModel.Axes.First(ax => ax.Tag == "Color") as LinearColorAxis;
                mainColorAxis.Palette = palette;
                mainColorAxis.HighColor = palette.Colors.Last(); // OxyColors.White,
                mainColorAxis.LowColor = palette.Colors.First();
                MinSensorRange = favPalette.LowerValue;
                MaxSensorRange = favPalette.UpperValue;
                mainColorAxis.AbsoluteMinimum = favPalette.LowerValue; // 최소 limit 값
                mainColorAxis.AbsoluteMaximum = favPalette.UpperValue; // 최대 limit 값
                MainPlotModel.InvalidatePlot(false);

                var overviewColorAxis = OverviewPlotModel.Axes.First(ax => ax.Tag == "Color") as LinearColorAxis;
                overviewColorAxis.Palette = palette;
                overviewColorAxis.HighColor = palette.Colors.Last(); // OxyColors.White,
                overviewColorAxis.LowColor = palette.Colors.First();
                MinSensorRange = favPalette.LowerValue;
                MaxSensorRange = favPalette.UpperValue;
                overviewColorAxis.AbsoluteMinimum = favPalette.LowerValue; // 최소 limit 값
                overviewColorAxis.AbsoluteMaximum = favPalette.UpperValue; // 최대 limit 값
                OverviewPlotModel.InvalidatePlot(false);
            }
        }

        [RelayCommand]
        private void SensorRangeChanged()
        {
            UpdatePaletteChanged();
        }

        private void TestPalettes()
        {
            OxyPalette pal = OxyPalette.Interpolate(
                4,
                OxyColors.Black,
                OxyColor.FromRgb(127, 0, 0),
                OxyColor.FromRgb(255, 127, 0),
                OxyColor.FromRgb(255, 255, 127),
                OxyColors.White);

        }

        private void UpdatePaletteChanged()
        {
            if (MainPlotModel.Series.Count == 0)
                return;
            
            var mainColorAxis = MainPlotModel.Axes.Single(s => s is LinearColorAxis) as LinearColorAxis;
            if(!string.IsNullOrEmpty(SelectedPaletteKey))
                SelectedPalette = Palettes[SelectedPaletteKey];
            
            mainColorAxis.Palette = SelectedPalette;
            mainColorAxis.AbsoluteMinimum = MinSensorRange; // 최소 limit 값
            mainColorAxis.AbsoluteMaximum = MaxSensorRange; // 최대 limit 값
            mainColorAxis.HighColor = SelectedPalette.Colors.Last(); // OxyColors.White,
            mainColorAxis.LowColor = SelectedPalette.Colors.First();
            MainPlotModel.InvalidatePlot(false);


            var overviewColorAxis = OverviewPlotModel.Axes.Single(s => s is LinearColorAxis) as LinearColorAxis;
            overviewColorAxis.Palette = SelectedPalette;
            overviewColorAxis.AbsoluteMinimum = MinSensorRange; // 최소 limit 값
            overviewColorAxis.AbsoluteMaximum = MaxSensorRange; // 최대 limit 값
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