using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;

namespace SRHWiscMano.Core.ViewModels
{
    /// <summary>
    /// Snapshot 들을 모두 표시하는 View의 ViewModel
    /// </summary>
    public partial class SnapshotViewViewModel : ViewModelBase, ISnapshotViewModel
    {
        public ObservableCollection<SnapshotViewModel> Snapshots { get; }

        [RelayCommand]
        private void SelectAll()
        {
            Snapshots.ToList().ForEach(sn => sn.IsSelected = true);
        }

        [RelayCommand]
        private void UnselectAll()
        {

        }

        [RelayCommand]
        private void NavigateToDetailView()
        {

        }
    }
}
