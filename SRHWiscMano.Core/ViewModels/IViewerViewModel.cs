using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OxyPlot;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.Services;

namespace SRHWiscMano.Core.ViewModels
{
    public interface IViewerViewModel
    {
        ITimeSeriesData TimeSeriesDataSource { get; }
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
        string ZoomLevel { get; set; }

        IRelayCommand FitToScreenCommand { get; }
        IRelayCommand<double> ZoomInCommand { get; }
        IRelayCommand<double> ZoomOutCommand { get; }
        IRelayCommand NavigateToSnapshotCommand { get; }

        IRelayCommand PrevSnapshotCommand { get; }
        IRelayCommand NextSnapshotCommand { get; }
        Dictionary<string, OxyPalette> Palettes { get; }

        OxyPalette SelectedPalette { get; }

        
    }
}