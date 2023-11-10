﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Printing;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using OxyPlot;
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
        private readonly ILogger<ViewerViewModel> logger;
        private readonly SharedService sharedService;

        public IExamination ExamData { get; private set; }
        public ObservableCollection<Note> Notes { get; }

        [ObservableProperty] private double minSensorData;

        [ObservableProperty] private double maxSensorData;
        public PlotModel MainPlotModel { get; private set; }
        public PlotController MainPlotController { get; }
        public PlotModel OverviewPlotModel { get; }
        public PlotController OverviewPlotController { get; }
        [ObservableProperty] private double zoomPercentage = 100;

        public IRelayCommand FitToScreenCommand { get; }
        public IRelayCommand PrevSnapshotCommand { get; private set; }
        public IRelayCommand NextSnapshotCommand { get; private set;}
        
        public Dictionary<string, OxyPalette> Palettes { get; private set; }


        [ObservableProperty] private OxyPalette selectedPalette;
        
        [ObservableProperty] private string selectedPaletteKey;
        

        public ViewerViewModel(ILogger<ViewerViewModel> logger, SharedService sharedService)
        {
            this.logger = logger;
            this.sharedService = sharedService;
            sharedService.ExamDataLoaded += SharedService_ExamDataLoaded;

            Palettes = PaletteUtils.GetPredefinedPalettes();

            MaxSensorData = 100;
            MinSensorData = -10;
        }

        private void SharedService_ExamDataLoaded(object? sender, EventArgs e)
        {

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
            if (Palettes.ContainsKey(paletteName))
            {
                SelectedPalette = Palettes[paletteName];
                SelectedPaletteKey = paletteName;
            }
        }

        /// <summary>
        /// Snapshot Page로 이동을 요청한다.
        /// </summary>
        [RelayCommand]
        private void NavigateToSnapshot()
        {
            WeakReferenceMessenger.Default.Send(new TabIndexChangeMessage(1));
        }
    }
}
