using Microsoft.Extensions.Options;
using SRHWiscMano.App.Data;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.App.ViewModels
{
    public class LoggerWindowViewModel : ViewModelBase
    {
        private readonly IOptions<AppSettings> settings;

        public LoggerWindowViewModel(IOptions<AppSettings> settings)
        {
            this.settings = settings;
        }

    }
}
