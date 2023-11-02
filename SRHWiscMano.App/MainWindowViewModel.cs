using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using ControlzEx.Theming;
using SRHWiscMano.App.ViewModels;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.App
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// Tab Index로 Tab 화면을 제어한다
        /// </summary>
        [ObservableProperty] private int selectedTabIndex;
        
        /// <summary>
        /// 생성자에서 가능한 Theme 를 로드하므로 ObservableCollection 은 해당이 안된다.
        /// </summary>
        public List<AppThemeMenu> AppThemes { get; set; }
        public List<AppThemeMenu> AppAccentThemes { get; set; }


        public MainWindowViewModel()
        {
            // ControlEx의 ThemeManager로부터 Theme를 불러온다.
            this.AppThemes = ThemeManager.Current.Themes
                .GroupBy(x => x.BaseColorScheme)
                .Select(x => x.First())
                .Select(a => new AppThemeMenu { Name = a.BaseColorScheme, BorderColorBrush = a.Resources["MahApps.Brushes.ThemeForeground"] as Brush, ColorBrush = a.Resources["MahApps.Brushes.ThemeBackground"] as Brush })
                .ToList();

            // create accent color menu items for the demo
            this.AppAccentThemes = ThemeManager.Current.Themes
                .GroupBy(x => x.ColorScheme)
                .OrderBy(a => a.Key)
                .Select(a => new AppThemeMenu { Name = a.Key, ColorBrush = a.First().ShowcaseBrush })
                .ToList();

        }
    }
}
