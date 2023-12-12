using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoreLinq;
using NodaTime;
using OxyPlot;
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

        #endregion

        /// <summary>
        /// Explorer View에서 보여줄 TimeFrameViewModel collection 이다.
        /// </summary>
        public ObservableCollection<TimeFrameViewModel> TimeFrames { get; } =
            new ObservableCollection<TimeFrameViewModel>();

        public ExplorerViewModel(ILogger<ExplorerViewModel> logger, SharedService sharedService,
            IOptions<AppSettings> settings,
            PaletteManager paletteManager)
        {
            this.logger = logger;
            this.sharedService = sharedService;
            this.paletteManager = paletteManager;
            this.settings = settings.Value;


            sharedService.ExamDataLoaded += SharedService_ExamDataLoaded;
        }


        private void SharedService_ExamDataLoaded(object? sender, EventArgs e)
        {
            var examData = sharedService.ExamData;
            var frameNotes = examData.Notes; //.ToList();

            frameNotes.Connect().Subscribe(changeSet =>
            {
                // Handle the initial set of items and any subsequent changes
                foreach (var change in changeSet)
                {
                    switch (change.Reason)
                    {
                        case ListChangeReason.Add:
                        {
                            var item = change.Item.Current;
                            logger.LogTrace($"Added: {change.Item.Current.Text}");
                            var startRow =
                                (int) Math.Round(item.Time.ToUnixTimeMilliseconds() -
                                                 settings.TimeFrameDurationInMillisecond / 2) / 10;
                            var endRow =
                                (int) Math.Round(item.Time.ToUnixTimeMilliseconds() +
                                                 settings.TimeFrameDurationInMillisecond / 2) / 10;
                            var notePlotData = PlotDataUtils.CreateSubRange(examData.PlotData, startRow, endRow, 0,
                                examData.PlotData.GetLength(1) - 1);
                            var timeFrame = new TimeFrame(item.Text, item.Time, notePlotData);
                            var timeFrameViewModel = new TimeFrameViewModel(timeFrame, item);
                            TimeFrames.Add(timeFrameViewModel);
                            logger.LogTrace($"Added in range: {item.Text}");
                            break;
                        }
                        case ListChangeReason.AddRange:
                        {
                            // Handling AddRange
                            foreach (var item in change.Range)
                            {
                                var startRow =
                                    (int) Math.Round(item.Time.ToUnixTimeMilliseconds() -
                                                     settings.TimeFrameDurationInMillisecond / 2) / 10;
                                var endRow =
                                    (int) Math.Round(item.Time.ToUnixTimeMilliseconds() +
                                                     settings.TimeFrameDurationInMillisecond / 2) / 10;
                                var notePlotData = PlotDataUtils.CreateSubRange(examData.PlotData, startRow, endRow, 0,
                                    examData.PlotData.GetLength(1) - 1);
                                var timeFrame = new TimeFrame(item.Text, item.Time, notePlotData);
                                var timeFrameViewModel = new TimeFrameViewModel(timeFrame, item);
                                TimeFrames.Add(timeFrameViewModel);
                                logger.LogTrace($"Added in range: {item.Text}");
                            }

                            break;
                        }
                        case ListChangeReason.Refresh:
                            logger.LogTrace($"Updated: {change.Item.Current.Text}");
                            break;
                        case ListChangeReason.Remove:
                            logger.LogTrace($"Removed: {change.Item.Current.Text}");
                            break;
                    }
                }
            });

            frameNotes.Add(new FrameNote(Instant.FromUnixTimeSeconds(10), "test time"));


            // foreach (var fNote in frameNotes)
            // {
            //     var startRow =
            //         (int) Math.Round(fNote.Time.ToUnixTimeMilliseconds() -
            //                          settings.TimeFrameDurationInMillisecond / 2) / 10;
            //     var endRow =
            //         (int) Math.Round(fNote.Time.ToUnixTimeMilliseconds() +
            //                          settings.TimeFrameDurationInMillisecond / 2) / 10;
            //     var notePlotData = PlotDataUtils.CreateSubRange(examData.PlotData, startRow, endRow, 0,
            //         examData.PlotData.GetLength(1) - 1);
            //
            //     var sensorRange = new Range<int>(0, examData.PlotData.GetLength(1) - 1);
            //     // var timeFrame = new TimeFrame(fNote.Text, examData, fNote.Text, fNote.Time, sensorRange, null, null, false, false, null, null, RegionsVersionType.UsesMP);
            // var timeFrame = new TimeFrame(fNote.Text, fNote.Time, notePlotData);
            // var timeFrameViewModel = new TimeFrameViewModel(timeFrame, fNote);
            // TimeFrames.Add(timeFrameViewModel);
            // }
        }


        [RelayCommand]
        private void SelectAll()
        {
            TimeFrames.ForEach(tf => tf.IsSelected = true);
            TimeFrames.ToList().ForEach(sn => sn.IsSelected = true);
        }

        [RelayCommand]
        private void UnselectAll()
        {
            TimeFrames.ForEach(tf => tf.IsSelected = false);
            TimeFrames.ToList().ForEach(sn => sn.IsSelected = false);
        }

        [RelayCommand]
        private void NavigateToDetailView()
        {
            logger.LogTrace($"Request navigate to Explorer view");
            WeakReferenceMessenger.Default.Send(new TabIndexChangeMessage(2));
        }

        [RelayCommand]
        private void AdjustLeft(object arg)
        {
            var timeFrame = (TimeFrameViewModel) arg;
            logger.LogTrace($"Explorer AdjustLeftCommand for {timeFrame.Label}");
        }

        [RelayCommand]
        private void AdjustRight(object arg)
        {
            var timeFrame = (TimeFrameViewModel) arg;
            logger.LogTrace($"Explorer AdjustRightCommand for {timeFrame.Label}");
        }

        [RelayCommand]
        private void ToggleChecked()
        {
            logger.LogTrace("Explorer ToggleCheckedCommand");
        }
    }
}