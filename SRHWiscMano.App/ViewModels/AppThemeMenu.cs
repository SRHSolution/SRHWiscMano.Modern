using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Theming;
using Microsoft.Extensions.Options;
using NLog;
using SRHWiscMano.App.Data;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.App.ViewModels
{
    public partial class AppThemeMenu : ViewModelBase
    {
        private readonly AppSettings? settings;

        [ObservableProperty] private string? name;

        [ObservableProperty] private Brush? borderColorBrush;

        [ObservableProperty] private Brush? colorBrush;

        public RelayCommand<string?> ChangeBaseColorCommand { get; }
        public RelayCommand<string?> ChangeAccentColorCommand { get; }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        public AppThemeMenu(IOptions<AppSettings> settings) : this()
        {
            this.settings = settings.Value;
        }

        public AppThemeMenu()
        {
            ChangeAccentColorCommand = new RelayCommand<string?>(DoChangeAccentTheme);
            ChangeBaseColorCommand = new RelayCommand<string?>(DoChangeBaseTheme);
        }

        private void DoChangeBaseTheme(string? name)
        {
            if (name is not null)
            {
                ThemeManager.Current.ChangeThemeBaseColor(Application.Current, name);
                if (settings != null)
                    settings.BaseTheme = name;
                Logger.Trace(name);
            }
        }

        private void DoChangeAccentTheme(string? name)
        {
            if (name is not null)
            {
                ThemeManager.Current.ChangeThemeColorScheme(Application.Current, name);
                if (settings != null)
                    settings.AccentTheme = name!;
                Logger.Trace(name);
            }
        }
    }
}
