using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ControlzEx.Theming;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using SRHWiscMano.App.Data;
using SRHWiscMano.App.Services;
using SRHWiscMano.App.Windows;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.Services;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.App.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly AppSettings settings;
        private readonly IImportService<IExamination> importService;
        private readonly SharedService sharedStorageService;
        private readonly ILogger<MainWindowViewModel> logger;

        /// <summary>
        /// Tab Index로 Tab 화면을 제어한다
        /// </summary>
        [ObservableProperty] private int selectedTabIndex;

        public ObservableCollection<RecentFile> RecentFiles{ get; } = new ObservableCollection<RecentFile>();

        /// <summary>
        /// 생성자에서 가능한 Theme 를 로드하므로 ObservableCollection 은 해당이 안된다.
        /// </summary>
        public List<AppThemeMenu> AppThemes { get; set; }

        public List<AppThemeMenu> AppAccentThemes { get; set; }

        /// <summary>
        /// 동적으로 Tabs을 제어할 수 있는 것을 테스트 하기 위함.
        /// </summary>
        public ObservableCollection<TabItem> Tabs { get; set; } = new ObservableCollection<TabItem>();

        public MainWindowViewModel(IOptions<AppSettings> settings, IImportService<IExamination> importService,
            SharedService sharedStorageService, ILogger<MainWindowViewModel> logger)
        {
            this.settings = settings.Value;
            this.importService = importService;
            this.sharedStorageService = sharedStorageService;
            this.logger = logger;


            settings.Value.RecentFiles?.ForEach(rf => RecentFiles.Add(new RecentFile(rf)));

            // ControlEx의 ThemeManager로부터 Theme를 불러온다.
            this.AppThemes = ThemeManager.Current.Themes
                .GroupBy(x => x.BaseColorScheme)
                .Select(x => x.First())
                .Select(a =>
                {
                    return new AppThemeMenu(settings)
                    {
                        Name = a.BaseColorScheme,
                        BorderColorBrush = a.Resources["MahApps.Brushes.ThemeForeground"] as Brush,
                        ColorBrush = a.Resources["MahApps.Brushes.ThemeBackground"] as Brush
                    };
                })
                .ToList();

            // create accent color menu items for the demo
            this.AppAccentThemes = ThemeManager.Current.Themes
                .GroupBy(x => x.ColorScheme)
                .OrderBy(a => a.Key)
                .Select(a =>
                {
                    return new AppThemeMenu(settings) 
                        { Name = a.Key, ColorBrush = a.First().ShowcaseBrush };
                })
                .ToList();



            WeakReferenceMessenger.Default.Register<TabIndexChangeMessage>(this, OnTabIndexChange);

            // Tabs.Add(new TabItem()
            // {
            //     Header = "Title1",
            //     Content = new ViewerView()
            // });
            //
            // Tabs.Add(new TabItem()
            // {
            //     Header = "Title2",
            //     Content = new Snapshots()
            // });
            //
            // Tabs.Add(new TabItem()
            // {
            //     Header = "Title3",
            //     Content = new ViewerView()
            // });
        }

        private void OnTabIndexChange(object recipient, TabIndexChangeMessage message)
        {
            var index = message.Value;
            SelectedTabIndex = index;
        }


        [RelayCommand]
        private void OpenFileBrowser()
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
                string filePath = openFileDialog.FileName;
                OpenFile(filePath);
            }
        }
        
        [RelayCommand]
        private void OpenRecentFile(string filePath)
        {
            OpenFile(filePath);
        }

        /// <summary>
        /// 입력된 경로의 파일을 불러온다
        /// </summary>
        /// <param name="filePath"></param>
        private void OpenFile(string filePath)
        {
            var examData = importService.ReadFromFile(filePath);

            if (examData == null)
            {
                logger.LogWarning("Exam data is not available");
                return;
            }

            AddToRecentFiles(filePath);

            logger.LogTrace("Exam data : {filename}", Path.GetFileName(filePath));
            sharedStorageService.SetExamData(examData);
        }


        /// <summary>
        /// 최신 파일 목록에 경로를 추가한다.
        /// </summary>
        /// <param name="filePath"></param>
        private void AddToRecentFiles(string filePath)
        {
            var fndItem = RecentFiles.FirstOrDefault(rf => rf.FilePath == filePath, null);
            if (fndItem != null)
            {
                RecentFiles.Remove(fndItem);
            }

            RecentFiles.Insert(0, new RecentFile(filePath));

            // Optional: Limit the number of recent files
            while (RecentFiles.Count > settings.MaxRecentFileSize)
            {
                RecentFiles.RemoveAt(RecentFiles.Count - 1);
            }

            settings.RecentFiles = RecentFiles.Select(sf => sf.FilePath).ToList();

            logger.LogTrace($"Added recent file : {RecentFiles[0].FileName}");
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