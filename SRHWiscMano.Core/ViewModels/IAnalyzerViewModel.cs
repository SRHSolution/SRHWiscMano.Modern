﻿using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.ViewModels
{
    public interface IAnalyzerViewModel
    {
        ObservableCollection<TimeFrameViewModel> TimeFrameViewModels { get; }
    }
}
