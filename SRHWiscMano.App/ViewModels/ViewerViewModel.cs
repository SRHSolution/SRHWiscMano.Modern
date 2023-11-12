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
        public PlotController MainPlotController { get; }
        public PlotModel OverviewPlotModel { get; }
        public PlotController OverviewPlotController { get; }

        [ObservableProperty] private double minSensorData;
        [ObservableProperty] private double maxSensorData;
        [ObservableProperty] private double zoomPercentage = 100;
        [ObservableProperty] private OxyPalette selectedPalette;
        [ObservableProperty] private string selectedPaletteKey;

        public IRelayCommand FitToScreenCommand { get; }
        public IRelayCommand PrevTimeFrameCommand { get; private set; }
        public IRelayCommand NextTimeFrameCommand { get; private set;}
        
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
                // else if(message.Value.Item1 == "Accent")
                // {
                //     var color = message.Value.Item2;
                //     MainPlotModel.TextColor = OxyColor.FromArgb(color.A, color.R, color.G, color.B);
                //     MainPlotModel.PlotAreaBorderColor = OxyColors.Gray;
                // }
            }
            
        }

        private void SharedService_ExamDataLoaded(object? sender, EventArgs e)
        {
            // int n = 100;
            // double x0 = -3.1;
            // double x1 = 3.1;
            // double y0 = -3;
            // double y1 = 3;
            // Func<double, double, double> peaks = (x, y) => 3 * (1 - x) * (1 - x) * Math.Exp(-(x * x) - (y + 1) * (y + 1)) - 10 * (x / 5 - x * x * x - y * y * y * y * y) * Math.Exp(-x * x - y * y) - 1.0 / 3 * Math.Exp(-(x + 1) * (x + 1) - y * y);
            // var xvalues = ArrayBuilder.CreateVector(x0, x1, 200);
            // var yvalues = ArrayBuilder.CreateVector(y0, y1, 100);
            // var peaksData = ArrayBuilder.Evaluate(peaks, xvalues, yvalues);
            //
            // var model = new PlotModel { Title = "Peaks" };
            // model.Axes.Add(new LinearColorAxis { Position = AxisPosition.Right, Palette = OxyPalettes.Rainbow(200) ?? OxyPalettes.Jet(500), HighColor = OxyColors.Gray, LowColor = OxyColors.Black });
            //
            // var hms = new HeatMapSeries { X0 = x0, X1 = x1, Y0 = y0, Y1 = y1, Data = peaksData };
            // model.Series.Add(hms);
            //
            // ((IPlotModel)this.MainPlotModel)?.AttachPlotView(null);
            // MainPlotModel = model;
            // MainPlotModel.InvalidatePlot(false);

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
            
            // Create your heatmap series and add to MyModel
            var heatmapSeries = new HeatMapSeries
            {
                X0 = 0,
                X1 = frameCount - 1, // Assuming 28 sensors
                Y0 = 0,
                Y1 = sensorCount - 1,
                Data = arrayData/* Your 2D data array */,
                Interpolate = true
            };


            var model = new PlotModel { Title = "Peaks" };
            model.Axes.Add(new LinearColorAxis { Position = AxisPosition.Right, Palette = OxyPalettes.Rainbow(200) ?? OxyPalettes.Jet(500), HighColor = OxyColors.Gray, LowColor = OxyColors.Black });

            model.Series.Add(heatmapSeries);
            ((IPlotModel)this.MainPlotModel)?.AttachPlotView(null);
            MainPlotModel = model;
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
            ZoomPercentage = (int)(ZoomPercentage*zoomVal);
        }

        [RelayCommand]
        private void FavoritePalette(string paletteName)
        {
            if (Palettes != null && Palettes.ContainsKey(paletteName))
            {
                logger.LogTrace($"Select favorite palette {paletteName}");
                SelectedPalette = Palettes[paletteName];
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
