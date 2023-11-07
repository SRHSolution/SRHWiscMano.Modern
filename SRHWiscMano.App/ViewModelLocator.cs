using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using SRHWiscMano.App.ViewModels;
using SRHWiscMano.App.Views;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.App
{
    public class ViewModelLocator
    {
        private readonly IServiceProvider _serviceProvider;

        public static void Initialize(IServiceProvider serviceProvider)
        {
            Current = new ViewModelLocator(serviceProvider);
        }

        public static ViewModelLocator Current { get; private set; }

        public ViewModelLocator()
        {
            // var checkpage = Ioc.Default.GetService<ManoDataView>();
        }

        public ViewModelLocator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IViewerViewModel ViewerViewModel => Ioc.Default.GetRequiredService<IViewerViewModel>();
        public ColorRangeSliderViewModel ColorRangeSlideViewModel => Ioc.Default.GetRequiredService<ColorRangeSliderViewModel>();
    }
}
