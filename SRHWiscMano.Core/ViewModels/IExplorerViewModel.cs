using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;

namespace SRHWiscMano.Core.ViewModels
{
    public interface IExplorerViewModel
    {
        ObservableCollection<TimeFrameViewModel> TimeFrames { get; }

        IRelayCommand SelectAllCommand { get; }
        IRelayCommand UnselectAllCommand { get; }
        IRelayCommand NavigateToDetailViewCommand { get; }
    }
}
