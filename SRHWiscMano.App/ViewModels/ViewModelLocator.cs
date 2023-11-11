using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using SRHWiscMano.App.Views;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.App.ViewModels
{
    public class ViewModelLocator
    {
        public MainWindowViewModel MainWindowViewModel => Ioc.Default.GetRequiredService<MainWindowViewModel>();
        public IViewerViewModel ViewerViewModel => Ioc.Default.GetRequiredService<IViewerViewModel>();
        public IExplorerViewModel ExplorerViewModel => Ioc.Default.GetRequiredService<IExplorerViewModel>();
        public IAnalyzerViewModel AnalyzerViewModel => Ioc.Default.GetRequiredService<IAnalyzerViewModel>();
        public ISettingViewModel SettingViewModel => Ioc.Default.GetRequiredService<ISettingViewModel>();
        public LoggerWindowViewModel LoggerWindowViewModel => Ioc.Default.GetRequiredService<LoggerWindowViewModel>();
        public ColorRangeSliderViewModel ColorRangeSlideViewModel => Ioc.Default.GetRequiredService<ColorRangeSliderViewModel>();
    }
}
