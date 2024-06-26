﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoreLinq;
using NodaTime;
using OxyPlot;
using OxyPlot.Annotations;
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
        private readonly SourceCache<ITimeFrame, int> timeFrames;

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
            // 이전에 있던 데이터를 connect로 연결해서 등록되어 있던 item에 대해 모두 call 될수 있도록 한다
            timeFrames.Connect().Subscribe(HandleTimeFrames);
        }

        /// <summary>
        /// SharedService의 TimeFrames에 등록된 데이터를 View에 binding 작업을 수행한다.
        /// </summary>
        /// <param name="changeSet"></param>
        private void HandleTimeFrames(IChangeSet<ITimeFrame, int> changeSet)
        {
            foreach (var change in changeSet)
            {
                switch (change.Reason)
                {
                    case ChangeReason.Add:
                    {
                        var insertIdx = timeFrames.Items.Index()
                            .FirstOrDefault(itm => itm.Value.Time > change.Current.Time,
                                new(TimeFrameViewModels.Count, null)).Key;

                        // TimeFrame -> TimeFrameViewModel을 생성한다.
                        var viewmodel = new TimeFrameViewModel(change.Current);
                        var framePlotModel = viewmodel.FramePlotModel;
                        var axisCenter = framePlotModel.Axes.First(ax => ax.Tag == "X").GetCenter();
                        var lineAnno = new LineAnnotation
                        {
                            Type = LineAnnotationType.Vertical,
                            X = axisCenter,
                            LineStyle = LineStyle.Solid,
                            Color = OxyColors.Gray,
                            ClipByYAxis = true,
                            Tag = change.Current.Id
                        };
                        viewmodel.FramePlotModel.Annotations.Add(lineAnno);
                        TimeFrameViewModels.Insert(insertIdx, viewmodel);

                        // TimeFrameViewModels에서 Label property 가 변경될 경우 이에 대한 Update 이벤트를 발생하도록 한다.
                        Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                                handler => (sender, e) => handler(e),
                                handler => viewmodel.PropertyChanged += handler,
                                handler => viewmodel.PropertyChanged -= handler)
                            .Where(x => x.PropertyName == nameof(viewmodel.Label)).Subscribe(
                                (arg) =>
                                {
                                    change.Current.Text = viewmodel.Label;
                                    timeFrames.AddOrUpdate(change.Current);
                                });
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

        [RelayCommand]
        private void SelectAll()
        {
            TimeFrameViewModels.ForEach(tf =>
            {
                if (tf.IsSelected == false)
                {
                    tf.IsSelected = true;
                    timeFrames.AddOrUpdate(tf.Data);
                }
            });

            logger.LogTrace($"SelectAll");
        }

        [RelayCommand]
        private void UnselectAll()
        {
            TimeFrameViewModels.ForEach(tf =>
            {
                if (tf.IsSelected )
                {
                    tf.IsSelected = false;
                    timeFrames.AddOrUpdate(tf.Data);
                }
            });


            logger.LogTrace($"UnselectAll");
        }

        /// <summary>
        /// Explorer view가 Loaded 되었을 때 실행된다
        /// </summary>
        [RelayCommand]
        private void NavigatedFrom()
        {
            logger.LogDebug("Explorer view is loaded");
        }

        [RelayCommand]
        private void NavigateToDetailView()
        {
            logger.LogTrace($"Request navigate to Explorer view");

            WeakReferenceMessenger.Default.Send(new TabIndexChangeMessage(2));
        }

        [RelayCommand]
        private void ToggleChecked(object arg)
        {
            var viewmodel = arg as TimeFrameViewModel;
            if (viewmodel.IsSelected)
            {
                var selectTxt = viewmodel.IsSelected == true ? "Selected" : "UnSelected";
                logger.LogTrace($"{viewmodel.Label} is {selectTxt}");
            }

            timeFrames.AddOrUpdate(viewmodel.Data);
        }

        [RelayCommand]
        private void AdjustLeft(object arg)
        {
            var viewmodel = arg as TimeFrameViewModel;
            viewmodel.AdjustTimeInMs(-10);
            timeFrames.AddOrUpdate(viewmodel.Data);
        }

        [RelayCommand]
        private void AdjustRight(object arg)
        {
            var viewmodel = arg as TimeFrameViewModel;
            viewmodel.AdjustTimeInMs(10);
            timeFrames.AddOrUpdate(viewmodel.Data);
        }
    }
}