using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SRHWiscMano.App.Data;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.App.ViewModels
{
    public class SettingViewModel : ViewModelBase
    {
        private readonly ILogger<SettingViewModel> logger;
        private readonly AppSettings setting;

        public SettingViewModel(IOptions<AppSettings> setting, ILogger<SettingViewModel> logger)
        {
            this.logger = logger;
            this.setting = setting.Value;
        }
    }
}
