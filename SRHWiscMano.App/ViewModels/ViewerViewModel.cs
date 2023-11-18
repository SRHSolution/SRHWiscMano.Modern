using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Printing;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using SRHWiscMano.App.Data;
using SRHWiscMano.App.Services;
using SRHWiscMano.App.Views;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.Services;
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
        [ObservableProperty] private string selectedPaletteKey;

        public IRelayCommand FitToScreenCommand { get; }
        public IRelayCommand PrevTimeFrameCommand { get; private set; }
        public IRelayCommand NextTimeFrameCommand { get; private set; }

        public Dictionary<string, OxyPalette> Palettes { get; }


        public ViewerViewModel(ILogger<ViewerViewModel> logger, SharedService sharedService)
        {
            this.logger = logger;
            this.sharedService = sharedService;
            sharedService.ExamDataLoaded += SharedService_ExamDataLoaded;

            Palettes = PaletteUtils.GetPredefinedPalettes();

            MaxSensorData = 100;
            MinSensorData = -10;

            MainPlotModel = new PlotModel { Title = "Sensor Data Heatmap" };



            WeakReferenceMessenger.Default.Register<AppBaseThemeChangedMessage>(this, ThemeChanged);
        }

        private void ThemeChanged(object recipient, AppBaseThemeChangedMessage message)
        {
            if (MainPlotModel != null)
            {
                var backColor = message.Value.Item1;
                var foreColor = message.Value.Item2;
                MainPlotModel.Background = OxyColor.FromArgb(backColor.A, backColor.R, backColor.G, backColor.B);
                MainPlotModel.TextColor = OxyColor.FromArgb(foreColor.A, foreColor.R, foreColor.G, foreColor.B);

                MainPlotModel.PlotAreaBorderColor = OxyColors.Gray;
            }
        }

        private void SharedService_ExamDataLoaded(object? sender, EventArgs e)
        {
            var examData = sharedService.ExamData;
            var sensorCount = examData.SensorCount();
            var frameCount = examData.Samples.Count;

            var arrayData = new double[frameCount, sensorCount];
            for (int i = 0; i < frameCount; i++)
            {
                for (int j = 0; j < sensorCount; j++)
                {
                    arrayData[i, j] = examData.Samples[i].Values[j];
                }
            }

            // 입력받은 Exam 데이터에서 최소 최대 값을 얻어 RangeSlider의 최소/최대 값을 변경한다
            MinSensorData = Math.Floor(arrayData.Cast<double>().Min());
            MaxSensorData = Math.Ceiling(arrayData.Cast<double>().Max());

            logger.LogInformation($"ExamData has min/max : {MinSensorData}/{MaxSensorData}");

            // 기존의 PlotView를 clear 한 후 ExamData에 대한 PlotModel을 생성해서 입력한다.
            ((IPlotModel)this.MainPlotModel)?.AttachPlotView(null);
            MainPlotModel = CreateMainPlotModel(examData, arrayData);
            
            ((IPlotModel)this.OverviewPlotModel)?.AttachPlotView(null);
            OverviewPlotModel = CreateOverviewPlotModel(examData, arrayData);
        }

        /// <summary>
        /// Main PlotView를 위한 PlotModel을 생성한다.
        /// </summary>
        /// <param name="examData"></param>
        /// <param name="plotData"></param>
        /// <returns></returns>
        private PlotModel CreateMainPlotModel(IExamination examData, double[,] plotData)
        {
            var sensorCount = examData.SensorCount();
            var frameCount = examData.Samples.Count;
            var model = new PlotModel { Title = "Peaks" };

            // Create your heatmap series and add to MyModel
            var heatmapSeries = new HeatMapSeries
            {
                X0 = 0,
                X1 = frameCount - 1, // Assuming 28 sensors
                Y0 = 0,
                Y1 = sensorCount - 1,
                Data = plotData /* Your 2D data array */,
                Interpolate = true
            };
            model.Series.Add(heatmapSeries);

            model.Axes.Add(new LinearColorAxis
            {
                Position = AxisPosition.Right,
                Palette = SelectedPalette,
                HighColor = SelectedPalette.Colors.Last(),// OxyColors.White,
                LowColor = SelectedPalette.Colors.First()
            });

            model.Axes.Add(new LinearAxis()
            {
                IsPanEnabled = false,
                IsZoomEnabled = false,
                Position = AxisPosition.Left,
                MaximumPadding = 0,
                MinimumPadding = 0,
                Minimum = 0,                // 초기 시작값
                Maximum = sensorCount - 1,  // 초기 최대값
                AbsoluteMinimum = 0,        // Panning 최소값
                AbsoluteMaximum = sensorCount - 1,  // Panning 최대값
                Tag = "Y"
            });

            // X-Axis
            model.Axes.Add(new LinearAxis()
            {
                // IsZoomEnabled = false,
                Position = AxisPosition.Bottom,
                MinimumPadding = 0,
                Minimum = 0,
                Maximum = 3000,// - 1,
                MajorStep = 100,
                AbsoluteMinimum = 0,
                AbsoluteMaximum = frameCount - 1,
                MinorStep = frameCount - 1, // 최대 범위를 입력하여 MinorStep 이 표시되지 않도록 한다
                Tag = "X"
            });

            return model;
        }

        /// <summary>
        /// Overivew PlotView를 위한 PlotModel을 생성한다.
        /// </summary>
        /// <param name="examData"></param>
        /// <param name="plotData"></param>
        /// <returns></returns>
        private PlotModel CreateOverviewPlotModel(IExamination examData, double[,] plotData)
        {
            var sensorCount = examData.SensorCount();
            var frameCount = examData.Samples.Count;
            var model = new PlotModel { Title = "Peaks" };

            // Create your heatmap series and add to MyModel
            var heatmapSeries = new HeatMapSeries
            {
                X0 = 0,
                X1 = frameCount - 1, // Assuming 28 sensors
                Y0 = 0,
                Y1 = sensorCount - 1,
                Data = plotData /* Your 2D data array */,
                Interpolate = true
            };
            model.Series.Add(heatmapSeries);

            model.Axes.Add(new LinearColorAxis
            {
                Position = AxisPosition.Right,
                Palette = SelectedPalette,
                HighColor = SelectedPalette.Colors.Last(),// OxyColors.White,
                LowColor = SelectedPalette.Colors.First()
            });

            model.Axes.Add(new LinearAxis()
            {
                IsPanEnabled = false,
                IsZoomEnabled = false,
                Position = AxisPosition.Left,
                MaximumPadding = 0,
                MinimumPadding = 0,
                Minimum = 0,                // 초기 시작값
                Maximum = sensorCount - 1,  // 초기 최대값
                AbsoluteMinimum = 0,        // Panning 최소값
                AbsoluteMaximum = sensorCount - 1,  // Panning 최대값
                Tag = "Y"
            });

            // X-Axis
            model.Axes.Add(new LinearAxis()
            {
                // IsZoomEnabled = false,
                Position = AxisPosition.Bottom,
                MinimumPadding = 0,
                Minimum = 0,
                Maximum = 3000,// - 1,
                MajorStep = 100,
                AbsoluteMinimum = 0,
                AbsoluteMaximum = frameCount - 1,
                MinorStep = frameCount - 1, // 최대 범위를 입력하여 MinorStep 이 표시되지 않도록 한다
                Tag = "X"
            });

            return model;
        }


        [RelayCommand]
        private void ZoomOut(double zoomVal)
        {
            logger.LogTrace($"Zoom : {zoomVal}");
            ZoomPercentage = (int)(ZoomPercentage / zoomVal);
        }

        [RelayCommand]
        private void ZoomIn(double zoomVal)
        {
            logger.LogTrace($"Zoom : {zoomVal}");
            ZoomPercentage = (int)(ZoomPercentage * zoomVal);
        }

        [RelayCommand]
        private void FavoritePalette(string paletteName)
        {
            if (Palettes != null && Palettes.ContainsKey(paletteName))
            {
                logger.LogTrace($"Select favorite palette {paletteName}");
                SelectedPalette = Palettes[paletteName];

                var colorAxis = MainPlotModel.Axes.Single(s => s is LinearColorAxis) as LinearColorAxis;
                colorAxis.Palette = SelectedPalette;
                colorAxis.AbsoluteMinimum = MinSensorRange; // 최소 limit 값
                colorAxis.AbsoluteMaximum = MaxSensorRange; // 최대 limit 값
                MainPlotModel.InvalidatePlot(false);


                SelectedPaletteKey = paletteName;
            }
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