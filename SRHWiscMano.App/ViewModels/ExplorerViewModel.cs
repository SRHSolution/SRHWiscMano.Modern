using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SRHWiscMano.App.Data;
using SRHWiscMano.App.Services;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.App.ViewModels
{
    /// <summary>
    /// TimeFrame 들을 모두 표시하는 View의 ViewModel
    /// </summary>
    public partial class ExplorerViewModel : ViewModelBase, IExplorerViewModel
    {
        #region Services

        private readonly ILogger<ExplorerViewModel> logger;
        private readonly SharedService sharedService;
        private readonly IOptions<AppSettings> settings;

        #endregion

        /// <summary>
        /// Explorer View에서 보여줄 TimeFrameViewModel collection 이다.
        /// </summary>
        public ObservableCollection<TimeFrameViewModel> TimeFrames { get; } = new ObservableCollection<TimeFrameViewModel>();
        
        
        public ExplorerViewModel(ILogger<ExplorerViewModel> logger, SharedService sharedService, IOptions<AppSettings> settings)
        {
            this.logger = logger;
            this.sharedService = sharedService;
            this.settings = settings;

            sharedService.ExamDataLoaded += SharedService_ExamDataLoaded;
        }

        private void SharedService_ExamDataLoaded(object? sender, EventArgs e)
        {
            var examData = sharedService.ExamData;
            var frameNotes = examData.Notes.ToList();

            foreach (var fNote in frameNotes)
            {
                // var timeFrame = new TimeFrame()
                // TimeFrames.Add(new TimeFrameViewModel());
            }

        }


        [RelayCommand]
        private void SelectAll()
        {
            TimeFrames.ToList().ForEach(sn => sn.IsSelected = true);
        }

        [RelayCommand]
        private void UnselectAll()
        {
            TimeFrames.ToList().ForEach(sn => sn.IsSelected = false);
        }

        [RelayCommand]
        private void NavigateToDetailView()
        {
            logger.LogTrace($"Request navigate to Explorer view");
            WeakReferenceMessenger.Default.Send(new TabIndexChangeMessage(2));
        }

        [RelayCommand]
        private void AdjustLeft()
        {
            logger.LogTrace("Explorer AdjustLeftCommand");
        }

        [RelayCommand]
        private void AdjustRight()
        {
            logger.LogTrace("Explorer AdjustRightCommand");
        }

        [RelayCommand]
        private void ToggleChecked()
        {
            logger.LogTrace("Explorer ToggleCheckedCommand");
        }
    }
}
