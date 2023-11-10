using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using OxyPlot;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.ViewModels
{
    public interface IViewerViewModel
    {
        IExamination ExamData { get; }

        ObservableCollection<Note> Notes { get; }

        double MinSensorData { get; }
        double MaxSensorData { get; }

        /// <summary>
        /// Main Plot Model
        /// </summary>
        PlotModel MainPlotModel { get; }

        /// <summary>
        /// Main Plot Controller
        /// </summary>
        PlotController MainPlotController { get; }

        PlotModel OverviewPlotModel { get; }
        PlotController OverviewPlotController { get; }


        double ZoomPercentage { get; set; }

        IRelayCommand FitToScreenCommand { get; }
        IRelayCommand<double> ZoomInCommand { get; }
        IRelayCommand<double> ZoomOutCommand { get; }
        IRelayCommand NavigateToExplorerCommand { get; }

        IRelayCommand PrevTimeFrameCommand { get; }
        IRelayCommand NextTimeFrameCommand { get; }
        Dictionary<string, OxyPalette> Palettes { get; }

        OxyPalette SelectedPalette { get; }

        string SelectedPaletteKey { get; }

        
    }
}