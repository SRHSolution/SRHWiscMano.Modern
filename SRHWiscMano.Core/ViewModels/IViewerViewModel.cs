﻿using System;
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
        IExamData ExamDataSource { get; }
        PlotModel DataPlotModel { get; }

        double ZoomPercentage { get; set; }
        string ZoomLevel { get; set; }

        RelayCommand<object> ObjectCommand { get; }
        RelayCommand<string> StringCommand { get; set; }
        RelayCommand<double> DoubleCommand { get; set; }

        RelayCommand PrevSnapshotCommand { get; }
        RelayCommand NextSnapshotCommand { get; }


        string SelectedPalette { get; }

        
    }
}