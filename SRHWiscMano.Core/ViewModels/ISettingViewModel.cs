using CommunityToolkit.Mvvm.Input;

namespace SRHWiscMano.Core.ViewModels
{
    public interface ISettingViewModel
    {
        IRelayCommand UpdateSettingsCommand { get; }
        IRelayCommand ReloadSettingsCommand { get; }
    }
}
