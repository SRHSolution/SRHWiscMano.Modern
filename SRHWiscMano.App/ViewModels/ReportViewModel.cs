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
    public partial class ReportViewModel : ViewModelBase, IReportViewModel
    {
        private readonly ILogger<ReportViewModel> logger;
        private readonly SharedService sharedService;
        [ObservableProperty] private TimeFrameViewModel timeFrame;

        public ReportViewModel(ILogger<ReportViewModel> logger, SharedService sharedService)
        {
            this.logger = logger;
            this.sharedService = sharedService;
        }
    }
}
