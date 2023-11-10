using CommunityToolkit.Mvvm.ComponentModel;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.ViewModels
{
    public partial class TimeFrameLabelsViewModel : ViewModelBase, ITimeFrameLabels
    {
        private readonly TimeFrameViewModel owner;
        [ObservableProperty] private string strategy;
        [ObservableProperty]private string texture;
        [ObservableProperty] private string volume;

        public TimeFrameLabelsViewModel(TimeFrameViewModel owner, ITimeFrameLabels initialState)
        {
            this.owner = owner;
            volume = initialState.Volume;
            texture = initialState.Texture;
            strategy = initialState.Strategy;
        }

        public string VolumeDisplay => volume ?? "?";
    }
}