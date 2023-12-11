using System;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ControlzEx.Theming;
using Microsoft.Extensions.Options;
using NLog;
using SRHWiscMano.App.Data;
using SRHWiscMano.Core.Data;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.App.ViewModels
{
    /// <summary>
    /// App Theme 메뉴 ViewModel
    /// </summary>
    public partial class AppThemeMenu : ViewModelBase
    {
        #region Services

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly AppSettings? settings;

        #endregion

        [ObservableProperty] private string? name;

        [ObservableProperty] private Brush? borderColorBrush;

        [ObservableProperty] private Brush? colorBrush;

        public RelayCommand<string?> ChangeBaseColorCommand { get; }
        public RelayCommand<string?> ChangeAccentColorCommand { get; }

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

                var backColor = (Color)Application.Current.Resources["MahApps.Colors.ThemeBackground"];
                var foreColor = (Color)Application.Current.Resources["MahApps.Colors.ThemeForeground"];

                settings.BaseBackColor = backColor;
                settings.BaseForeColor = foreColor;

                WeakReferenceMessenger.Default.Send(
                    new AppBaseThemeChangedMessage(new Tuple<Color, Color>(backColor, foreColor)));
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

                var color = (Color)Application.Current.Resources["MahApps.Colors.Accent"];
                WeakReferenceMessenger.Default.Send(new AppSchemeColorChangedMessage(color));

                settings.AccentThemeColor = color;
                Logger.Trace(name);
            }
        }
    }
}