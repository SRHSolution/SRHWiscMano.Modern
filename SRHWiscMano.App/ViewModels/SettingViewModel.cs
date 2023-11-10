using System.IO;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SRHWiscMano.App.Data;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.App.ViewModels
{
    public partial class SettingViewModel : ViewModelBase
    {
        private readonly ILogger<SettingViewModel> logger;
        private readonly AppSettings settings;

        public SettingViewModel(IOptions<AppSettings> settings, ILogger<SettingViewModel> logger)
        {
            this.logger = logger;
            this.settings = settings.Value;
        }

        /// <summary>
        /// 수정한 Setting 값을 File에 업데이트 저장한다
        /// </summary>
        [RelayCommand]
        private void UpdateSettings()
        {
            var configFilePath = settings.FilePath;

            // config 파일에 다른 section 이 있는 것을 감안하여 AppSettings Section만 업데이트 하도록 한다.
            var jsonConfig = JObject.Parse(File.ReadAllText(configFilePath));
            jsonConfig[nameof(settings)] = JObject.Parse(JsonConvert.SerializeObject(settings, Formatting.Indented));
            File.WriteAllText(configFilePath, jsonConfig.ToString(Formatting.Indented));
        }

        /// <summary>
        /// Configuration 값을 다시 reload 하여 적용한다.
        /// </summary>
        [RelayCommand]
        private void ReloadSettings()
        {
            var config = new ConfigurationBuilder().AddJsonFile(settings.FilePath).Build();
            config.Bind(settings);
        }
    }
}
