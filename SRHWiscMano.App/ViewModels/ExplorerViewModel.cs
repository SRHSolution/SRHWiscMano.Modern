using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.App.ViewModels
{
    /// <summary>
    /// TimeFrame 들을 모두 표시하는 View의 ViewModel
    /// </summary>
    public partial class ExplorerViewModel : ViewModelBase, IExplorerViewModel
    {
        public ObservableCollection<TimeFrameViewModel> TimeFrames { get; }

        [RelayCommand]
        private void SelectAll()
        {
            TimeFrames.ToList().ForEach(sn => sn.IsSelected = true);
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
