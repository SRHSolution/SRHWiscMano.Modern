using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using OxyPlot;
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
        private readonly ILogger<ViewerViewModel> logger;
        private readonly SharedService sharedService;

        public ITimeSeriesData TimeSeriesDataSource { get; private set; }
        
        [ObservableProperty] private double minSensorData;

        [ObservableProperty] private double maxSensorData;
        public PlotModel MainPlotModel { get; private set; }
        public PlotController MainPlotController { get; }
        public PlotModel OverviewPlotModel { get; }
        public PlotController OverviewPlotController { get; }
        public double ZoomPercentage { get; set; } = 0.5;

        public RelayCommand FitToScreenCommand { get; }
        public RelayCommand<double> ZoomInCommand { get; }
        public RelayCommand<double> ZoomOutCommand { get; }
        public RelayCommand PrevSnapshotCommand { get; private set; }
        public RelayCommand NextSnapshotCommand { get; private set;}
        public Dictionary<string, OxyPalette> Palettes { get; private set; }


        [ObservableProperty] private OxyPalette selectedPalette;

        [ObservableProperty] private string zoomLevel = "10.0";
        private RelayCommand<object> zoomInCommand;

        public ViewerViewModel(ILogger<ViewerViewModel> logger, SharedService sharedService)
        {
            this.logger = logger;
            this.sharedService = sharedService;
            sharedService.ExamDataLoaded += SharedService_ExamDataLoaded;


            Palettes = PaletteUtils.GetPredefinedPalettes();
            ZoomInCommand = new RelayCommand<double>(OnZoomIn);
            ZoomOutCommand = new RelayCommand<double>(OnZoomOut);

            MaxSensorData = 100;
            MinSensorData = -10;
        }

        private void SharedService_ExamDataLoaded(object? sender, EventArgs e)
        {
            logger.LogInformation("ExamData Loaded");
        }

        private void OnZoomOut(double zoomVal)
        {
            logger.LogTrace($"Zoom : {zoomVal}");
        }

        private void OnZoomIn(double zoomVal)
        {
            logger.LogTrace($"Zoom : {zoomVal}");
        }

        [RelayCommand]
        private void SelectedPaletteChanged(SelectionChangedEventArgs arg)
        {
            
        }
    }
}
