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
using SRHWiscMano.App.Views;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.Services;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.App.ViewModels
{
    public partial class ViewerViewModel : ViewModelBase, IViewerViewModel
    {
        private readonly IImportService<IExamData> importer;
        private readonly ILoggerService logger;

        public IExamData ExamDataSource { get; private set; }
        public PlotModel DataPlotModel { get; private set; }
        public double ZoomPercentage { get; set; } = 0.5;
        public RelayCommand<object> ZoomInCommand { get; private set; }
        public RelayCommand<string> ZoomOutCommand { get; set; }
        public RelayCommand<double> ZoomDoubleCommand { get; set; }
        public RelayCommand PrevSnapshotCommand { get; private set; }
        public RelayCommand NextSnapshotCommand { get; private set;}
        
        private Dictionary<string, OxyPalette> Palettes { get; set; }

        [ObservableProperty] private string selectedPalette;

        [ObservableProperty] private string zoomLevel;

        public ViewerViewModel(ILoggerFactory loggerFactory, IImportService<IExamData> importer)
        {
            this.importer = importer;
            logger = loggerFactory.CreateLogger<ViewerViewModel>();

            Palettes = PaletteUtils.GetPredefinedPalettes();
            ZoomInCommand = new RelayCommand<object>(OnZoomIn);
            ZoomOutCommand = new RelayCommand<string>(OnZoomOut);
            ZoomDoubleCommand = new RelayCommand<double>(OnZoomDouble);
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
             ExamDataSource = importer.ReadFromFile(filePath);
        }
    }
}
