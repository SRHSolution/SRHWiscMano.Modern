using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using SRHWiscMano.App.Services;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.App.ViewModels
{
    public partial class AnalyzerViewModel : ViewModelBase, IAnalyzerViewModel
    {
        private readonly ILogger<AnalyzerViewModel> logger;
        private readonly SharedService sharedService;
        [ObservableProperty] private TimeFrameViewModel timeFrame;

        public AnalyzerViewModel(ILogger<AnalyzerViewModel> logger, SharedService sharedService)
        {
            this.logger = logger;
            this.sharedService = sharedService;
        }
    }
}
