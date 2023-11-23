using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using OxyPlot;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.ViewModels
{
    public interface IViewerViewModel
    {
        IExamination ExamData { get; }

        ObservableCollection<FrameNote> Notes { get; }

        double MinSensorData { get; }
        double MaxSensorData { get; }

        double MinSensorRange { get; }
        double MaxSensorRange { get; }

        IRelayCommand SensorRangeChangedCommand { get; }

        public bool ImageVisibility { get; }

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
        IRelayCommand<FavoritePalette> FavoritePaletteCommand { get; }
        IRelayCommand NavigateToExplorerCommand { get; }

        IRelayCommand PrevFrameNoteCommand { get; }
        IRelayCommand NextFrameNoteCommand { get; }
        Dictionary<string, OxyPalette> Palettes { get; }

        OxyPalette SelectedPalette { get; }

        string SelectedPaletteKey { get; }

        double InterpolateSensorScale { get; }

        
    }
}