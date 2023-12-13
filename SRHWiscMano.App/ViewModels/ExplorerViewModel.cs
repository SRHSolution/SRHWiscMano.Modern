using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoreLinq;
using NodaTime;
using ReactiveUI;
using SRHWiscMano.App.Data;
using SRHWiscMano.App.Services;
using SRHWiscMano.Core.Helpers;
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
        private readonly PaletteManager paletteManager;
        private readonly AppSettings settings;
        private readonly SourceCache<TimeFrame, int> timeFrames;

        #endregion

        /// <summary>
        /// Explorer View에서 보여줄 TimeFrameViewModel collection 이다.
        /// </summary>
        public ObservableCollection<TimeFrameViewModel> TimeFrameViewModels { get; } = new();

        public ExplorerViewModel(ILogger<ExplorerViewModel> logger, SharedService sharedService,
            IOptions<AppSettings> settings,
            PaletteManager paletteManager)
        {
            this.logger = logger;
            this.sharedService = sharedService;
            this.paletteManager = paletteManager;
            this.settings = settings.Value;
            timeFrames = sharedService.TimeFrames; 
            timeFrames.Connect().Subscribe((Action<IChangeSet<TimeFrame, int>>) HandleTimeFrames2);
            sharedService.ExamDataLoaded += SharedService_ExamDataLoaded;
        }

        private void HandleTimeFrames2(IChangeSet<TimeFrame, int> changeSet)
        {
            foreach (var change in changeSet)
            {
                switch (change.Reason)
                {
                    case ChangeReason.Add:
                    {
                        var insertIdx = timeFrames.Items.Index()
                            .FirstOrDefault(itm => itm.Value.Time > change.Current.Time, new (TimeFrameViewModels.Count, null)).Key;
                        TimeFrameViewModels.Insert(insertIdx, new TimeFrameViewModel(change.Current));
                        break;
                    }
                    case ChangeReason.Remove:
                    {
                        TimeFrameViewModels.Remove(
                            TimeFrameViewModels.SingleOrDefault(item => item.Id == change.Current.Id));
                        break;
                    }
                    case ChangeReason.Moved:
                        break;
                    
                    // 다른 view에서 변경하는 것은 Time 밖에 없으므로 PlotData를 update한다.
                    case ChangeReason.Update:
                        var updItem = TimeFrameViewModels.SingleOrDefault(item => item.Id == change.Current.Id);
                        updItem.RefreshPlotData();
                        break;

                    case ChangeReason.Refresh:
                        
                        break;
                }
            }
        }

        private void HandleTimeFrames(IChangeSet<TimeFrame> changeSet)
        {
            // Handle the initial set of items and any subsequent changes
            foreach (var change in changeSet)
            {
                switch (change.Reason)
                {
                    case ListChangeReason.Add:
                    {
                        TimeFrameViewModels.Insert(change.Item.CurrentIndex, new TimeFrameViewModel(change.Item.Current));
                        break;
                    }
                    case ListChangeReason.AddRange:
                    {
                        // Handling AddRange
                        foreach (var item in change.Range)
                        {
                            TimeFrameViewModels.Add(new TimeFrameViewModel(item));
                            logger.LogTrace($"Added in range: {item.Text}");
                        }

                        break;
                    }
                    case ListChangeReason.Refresh:
                        logger.LogTrace($"Updated: {change.Item.Current.Text}");
                        break;
                    case ListChangeReason.Remove:
                        logger.LogTrace($"Removed: {change.Item.Current.Text}");
                        var itemToRem = TimeFrameViewModels.Single(tf => tf.Label == change.Item.Current.Text);
                        TimeFrameViewModels.Remove(itemToRem);
                        break;

                    case ListChangeReason.RemoveRange:
                        break;
                    case ListChangeReason.Clear:
                        TimeFrameViewModels.Clear();
                        break;
                }
            }
        }

        private void SharedService_ExamDataLoaded(object? sender, EventArgs e)
        {
            var editItem = timeFrames.Items.ElementAt(0);
            editItem.Text = "modified";
            timeFrames.AddOrUpdate(editItem);

            var newFrame = sharedService.CreateTimeFrame("Test time2", Instant.FromUnixTimeSeconds(100));
            var newIndex = timeFrames.Items.Index().FirstOrDefault(itm => itm.Value.Time < newFrame.Time).Key;

            timeFrames.AddOrUpdate(newFrame);
        }


        [RelayCommand]
        private void SelectAll()
        {
            TimeFrameViewModels.ForEach(tf => tf.IsSelected = true);
            TimeFrameViewModels.ToList().ForEach(sn => sn.IsSelected = true);
            
            logger.LogTrace($"SelectAll");
        }

        [RelayCommand]
        private void UnselectAll()
        {
            TimeFrameViewModels.ForEach(tf => tf.IsSelected = false);
            TimeFrameViewModels.ToList().ForEach(sn => sn.IsSelected = false);
            
            logger.LogTrace($"UnselectAll");
        }

        [RelayCommand]
        private void NavigateToDetailView()
        {
            logger.LogTrace($"Request navigate to Explorer view");
            
            WeakReferenceMessenger.Default.Send(new TabIndexChangeMessage(2));
        }

        [RelayCommand]
        private void ToggleChecked()
        {
            logger.LogTrace("Explorer ToggleCheckedCommand");
        }

        [RelayCommand]
        private void AdjustLeft(object arg)
        {
            var viewmodel = arg as TimeFrameViewModel;
            viewmodel.AdjustTimeInMs(-100);
        }

        [RelayCommand]
        private void AdjustRight(object arg)
        {
            var viewmodel = arg as TimeFrameViewModel;
            viewmodel.AdjustTimeInMs(100);

            // timeFrames.AddOrUpdate(viewmodel.Data);
            // viewmodel.Data
            
        }


    }
}