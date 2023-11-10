using CommunityToolkit.Mvvm.ComponentModel;
using NodaTime;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.ViewModels
{
    /// <summary>
    /// 센서 데이터에서 분석을 위한 Snapshot 을 지정하고 이를 View 표기하기 위한 ViewModel
    /// </summary>
    public partial class SnapshotViewModel : ViewModelBase, ISnapshot
    {

        private readonly ISnapshot snapshot;
        
        public SnapshotViewModel(ISnapshot snapshot)
        {
            this.snapshot = snapshot;
        }


        public string Id => snapshot.Id;
        public IExamination Data { get; }
        public string Text { get; }
        public Instant Time { get; }
        public Range<int> SensorRange { get; }
        public int? VPUpperBound { get; }
        public int? UesLowerBound { get; }
        
        [ObservableProperty] private bool isSelected;
        public bool NormalEligible { get; }

        public ISnapshotLabels Labels { get; }
        public IReadOnlyList<IRegion> Regions { get; }
        public RegionsVersion RegionsVersion { get; }
    }
}