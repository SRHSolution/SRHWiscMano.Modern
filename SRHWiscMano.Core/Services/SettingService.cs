using Microsoft.Extensions.Configuration;
using System.IO;
using Newtonsoft.Json;

namespace SRHWiscMano.Core.Services
{
    /// <summary>
    /// IConfiguration 과 config file path 값을 받아서 변경된 Setting 값을 파일에 업데이트 하는 것을 목적으로 구현함.
    /// 하지만, services.Configure<AppSettings>( config.GetSection("AppSettings")) 와 같은 방식으로 하였을 때 config 값이 자동 업데이트 되므로
    /// 이를 활용하며, 필요시에 json 파일을 업데이트 하는 방식으로 구현한다.
    /// </summary>
    /// <typeparam name="TSettings"></typeparam>
    [Obsolete]
    public class SettingsService<TSettings> where TSettings : class
    {
        private readonly IConfiguration _configuration;
        private readonly string _configFilePath;
        private TSettings? _settings;

        public SettingsService(IConfiguration configuration, string configFilePath)
        {
            _configuration = configuration;
            _configFilePath = configFilePath;
            LoadUserSettings();
        }

        private void LoadUserSettings()
        {
            _settings= _configuration.Get<TSettings>();
        }

        private void SaveUserSettings()
        {
            var json = JsonConvert.SerializeObject(_settings);
            File.WriteAllText(_configFilePath, json);
        }

        public TSettings GetUserSettings()
        {
            return _settings;
        }

        public void UpdateUserSettings(Action<TSettings> updateAction)
        {
            updateAction(_settings);
            SaveUserSettings();
        }
    }

}
        