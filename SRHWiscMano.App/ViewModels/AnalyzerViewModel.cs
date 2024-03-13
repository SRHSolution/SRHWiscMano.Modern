using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoreLinq.Extensions;
using OxyPlot;
using SRHWiscMano.App.Data;
using SRHWiscMano.App.Services;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.App.ViewModels
{
    public partial class AnalyzerViewModel : ViewModelBase, IAnalyzerViewModel
    {
        private readonly ILogger<AnalyzerViewModel> logger;
        private readonly SharedService sharedService;
        private readonly IOptions<AppSettings> settings;
        private readonly SourceCache<ITimeFrame, int> timeFrames;

        [ObservableProperty] private PlotModel mainPlotModel;
        [ObservableProperty] private PlotController mainPlotController;

        [ObservableProperty] private PlotModel graphPlotModel;
        [ObservableProperty] private PlotController graphPlotController;

        [ObservableProperty] private string statusMessage = "Test status message";

        [ObservableProperty] private int selectedIndexOfTimeFrameViewModel = 0;

        public ObservableCollection<TimeFrameViewModel> TimeFrameViewModels { get; } = new();

        public AnalyzerViewModel(ILogger<AnalyzerViewModel> logger, SharedService sharedService,
            IOptions<AppSettings> settings)
        {
            this.logger = logger;
            this.sharedService = sharedService;
            this.settings = settings;

            timeFrames = sharedService.TimeFrames;
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
                            .FirstOrDefault(itm => itm.Value.Time > change.Current.Time, new(TimeFrameViewModels.Count, null)).Key;
                        var viewmodel = new TimeFrameViewModel(change.Current);
                        viewmodel.FramePlotController = new PlotController();
                        viewmodel.FramePlotController.UnbindAll();

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

        /// <summary>
        /// View의 TimeFrames listview 에서 선택된 item 객체를 받는다.
        /// </summary>
        /// <param name="selectedItem"></param>
        [RelayCommand]
        private void SelectionChanged(object selectedItem)
        {
            try
            {
                var timeFrameData = (selectedItem as TimeFrameViewModel).Data;
                var timeFrameClone = new TimeFrameViewModel(timeFrameData);
                MainPlotModel = timeFrameClone.FramePlotModel;
                var timeFrameGraph = new TimeFrameGraphViewModel(timeFrameData);
                GraphPlotModel = timeFrameGraph.FramePlotModel;
            }
            catch
            {
                logger.LogError("Selecting timeframe viewmodel got error");
            }
        }

        [RelayCommand]
        private void PreviousTimeFrame(int selectedIndex)
        {
            if (selectedIndex > 0)
                SelectedIndexOfTimeFrameViewModel -= 1;

            
        }

        [RelayCommand]
        private void NextTimeFrame(int selectedIndex)
        {
            if (selectedIndex < TimeFrameViewModels.Count-1)
                SelectedIndexOfTimeFrameViewModel += 1;
        }
    }
}
