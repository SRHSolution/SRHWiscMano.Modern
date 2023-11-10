using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRHWiscMano.App.ViewModels
{
    public interface ISettingViewModel
    {
        IRelayCommand UpdateSettingsCommand { get; }
        IRelayCommand ReloadSettingsCommand { get; }
    }
}
