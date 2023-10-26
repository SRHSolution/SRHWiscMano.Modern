using CommunityToolkit.Mvvm.ComponentModel;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.ViewModels
{
    public partial class SnapshotLabelsViewModel : ViewModelBase, ISnapshotLabels
    {
        private readonly SnapshotViewModel owner;
        [ObservableProperty] private string strategy;
        [ObservableProperty]private string texture;
        [ObservableProperty] private string volume;

        public SnapshotLabelsViewModel(SnapshotViewModel owner, ISnapshotLabels initialState)
        {
            this.owner = owner;
            volume = initialState.Volume;
            texture = initialState.Texture;
            strategy = initialState.Strategy;
        }

        public string VolumeDisplay => volume ?? "?";
    }
}