using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Theming;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using NLog;
using SRHWiscMano.App.Services;
using SRHWiscMano.App.ViewModels;
using SRHWiscMano.App.Windows;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.Services;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.App
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly IImportService<ITimeSeriesData> importService;
        private readonly SharedService sharedStorageService;
        private readonly ILogger<MainWindowViewModel> logger;

        /// <summary>
        /// Tab Index로 Tab 화면을 제어한다
        /// </summary>
        [ObservableProperty] private int selectedTabIndex;

        /// <summary>
        /// 생성자에서 가능한 Theme 를 로드하므로 ObservableCollection 은 해당이 안된다.
        /// </summary>
        public List<AppThemeMenu> AppThemes { get; set; }
        public List<AppThemeMenu> AppAccentThemes { get; set; }


        public MainWindowViewModel(IImportService<ITimeSeriesData> importService, SharedService sharedStorageService, ILogger<MainWindowViewModel> logger)
        {
            this.importService = importService;
            this.sharedStorageService = sharedStorageService;
            this.logger = logger;


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


        [RelayCommand]
        private void OpenFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                // Set filter for file extension and default file extension 
                DefaultExt = ".txt",
                Filter = "Text documents (.txt)|*.txt"
            };

            // Display OpenFileDialog by calling ShowDialog method 
            bool? result = openFileDialog.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = openFileDialog.FileName;
                var examData = importService.ReadFromFile(filename);

                logger.LogTrace("Exam data : {filename}", Path.GetFileName(filename));
                sharedStorageService.SetExamData(examData);

            }
        }

        /// <summary>
        /// Logger single instance 를 불러와서 윈도우 창을 출력한다
        /// </summary>
        [RelayCommand]
        private void ShowLogger()
        {
            Ioc.Default.GetRequiredService<LoggerWindow>().Show();
        }
    }
}
