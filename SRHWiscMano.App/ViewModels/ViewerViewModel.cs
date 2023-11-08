using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        private readonly SharedService sharedService;
        private readonly IImportService<ITimeSeriesData> importer;
        private readonly ILoggerService logger;

        public ITimeSeriesData TimeSeriesDataSource { get; private set; }
        
        [ObservableProperty] private double minSensorData;

        [ObservableProperty] private double maxSensorData;
        public PlotModel MainPlotModel { get; private set; }
        public PlotController MainPlotController { get; }
        public PlotModel OverviewPlotModel { get; }
        public PlotController OverviewPlotController { get; }
        public double ZoomPercentage { get; set; } = 0.5;
        public RelayCommand<object> ObjectCommand { get; private set; }
        public RelayCommand<string> StringCommand { get; set; }
        public RelayCommand<double> DoubleCommand { get; set; }
        public RelayCommand FitToScreenCommand { get; }
        public RelayCommand ZoomInCommand { get; }
        public RelayCommand ZoomOutCommand { get; }
        public RelayCommand PrevSnapshotCommand { get; private set; }
        public RelayCommand NextSnapshotCommand { get; private set;}
        public Dictionary<string, OxyPalette> Palettes { get; private set; }



        [ObservableProperty] private OxyPalette selectedPalette;

        [ObservableProperty] private string zoomLevel = "10.0";

        public ViewerViewModel(ILoggerFactory loggerFactory, SharedService sharedService)
        {
            this.importer = importer;
            this.sharedService = sharedService;
            sharedService.ExamDataLoaded += SharedService_ExamDataLoaded;

            logger = loggerFactory.CreateLogger<ViewerViewModel>();

            Palettes = PaletteUtils.GetPredefinedPalettes();
            ObjectCommand = new RelayCommand<object>(OnZoomIn);
            StringCommand = new RelayCommand<string>(OnZoomOut);
            DoubleCommand = new RelayCommand<double>(OnZoomDouble);

            MaxSensorData = 100;
            MinSensorData = -10;
        }

        private void SharedService_ExamDataLoaded(object? sender, EventArgs e)
        {
            logger.LogInformation("ExamData Loaded");
        }

        private void OnZoomDouble(double obj)
        {

        }

        private void OnZoomOut(string? obj)
        {
            // logger.LogInformation($"Got {obj}");
        }

        private void OnZoomIn(object? obj)
        {
            ;
        }

        [RelayCommand]
        private void SelectedPaletteChanged(SelectionChangedEventArgs arg)
        {
            
        }

        public void ImportExamData(string filePath)
        {
             TimeSeriesDataSource = importer.ReadFromFile(filePath);
        }
    }
}
