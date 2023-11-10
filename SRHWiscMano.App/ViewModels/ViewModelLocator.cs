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
        public ViewModelLocator()
        {
        }

        public IViewerViewModel ViewerViewModel => Ioc.Default.GetRequiredService<IViewerViewModel>();
        
        public ColorRangeSliderViewModel ColorRangeSlideViewModel => Ioc.Default.GetRequiredService<ColorRangeSliderViewModel>();
    }
}
