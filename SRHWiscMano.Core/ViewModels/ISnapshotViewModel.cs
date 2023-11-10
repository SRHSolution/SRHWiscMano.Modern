using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;

namespace SRHWiscMano.Core.ViewModels
{
    interface ISnapshotViewModel
    {
        ObservableCollection<SnapshotViewModel> Snapshots { get; }

        IRelayCommand SelectAllCommand { get; }
        IRelayCommand UnselectAllCommand { get; }
        IRelayCommand NavigateToDetailViewCommand { get; }
    }
}
