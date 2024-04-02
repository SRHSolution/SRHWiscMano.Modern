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
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoreLinq.Extensions;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using SRHWiscMano.App.Data;
using SRHWiscMano.App.Services;
using SRHWiscMano.Core.Data;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.Services.Detection;
using SRHWiscMano.Core.ViewModels;


namespace SRHWiscMano.App.ViewModels
{
    public partial class AnalyzerViewModel : ViewModelBase, IAnalyzerViewModel
    {
        private readonly ILogger<AnalyzerViewModel> logger;
        private readonly SharedService sharedService;
        private readonly IOptions<AppSettings> settings;
        private readonly IRegionFinder regionFinder;
        private readonly SourceCache<ITimeFrame, int> timeFrames;

        [ObservableProperty] private PlotModel mainPlotModel;
        [ObservableProperty] private PlotController mainPlotController;

        [ObservableProperty] private PlotModel graphPlotModel;
        [ObservableProperty] private PlotController graphPlotController;

        [ObservableProperty] private string statusMessage = "Test status message";

        [ObservableProperty] private int selectedIndexOfTimeFrameViewModel = -1;

        public ObservableCollection<TimeFrameViewModel> TimeFrameViewModels { get; } = new();
        
        /// <summary>
        /// Heatmap clicked 이벤트에 대한 delegate를 등록하는 변수
        /// </summary>
        public DelegatePlotCommand<OxyMouseDownEventArgs> HeatmapClicked { get; set; }
        public AnalyzerViewModel()
        {
            
        }

        public AnalyzerViewModel(ILogger<AnalyzerViewModel> logger, SharedService sharedService,
            IOptions<AppSettings> settings, IRegionFinder regionFinder)
        {
            this.logger = logger;
            this.sharedService = sharedService;
            this.settings = settings;
            this.regionFinder = regionFinder;

            timeFrames = sharedService.TimeFrames;
            timeFrames.Connect().Subscribe(HandleTimeFrames);

            WeakReferenceMessenger.Default.Register<SensorBoundsChangedMessage>(this, SensorBoundsChanged);
        }

        


        /// <summary>
        /// 
        /// </summary>
        [RelayCommand]
        private void ListItemsLoaded()
        {
            if (timeFrames.Count > 0 && SelectedIndexOfTimeFrameViewModel < 0)
            {
                SelectedIndexOfTimeFrameViewModel = 0;
            }
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
                        
                        // TODO : ListBox내의 item 은 업데이트 되지만, 하단의 mainplotmodel, graphmodel은 업데이트도 필요함
                        if (SelectedIndexOfTimeFrameViewModel == change.Current.Id)
                        {
                            // MainViewmodel, GraphPlotModel UPDATE
                        }
                        break;

                    case ChangeReason.Refresh:

                        break;
                }
            }
        }

        private PlotController BuildPlotcontroller()
        {
            var plotController = new PlotController();
            // plotController.UnbindAll();
            HeatmapClicked =
                new DelegatePlotCommand<OxyMouseDownEventArgs>((view, controller, args) =>
                    HeatmapClickedCommand(view, args));

            // plotController.BindMouseDown(OxyMouseButton.Left, OxyModifierKeys.None, HeatmapClicked);
            plotController.BindMouseDown(OxyMouseButton.Left, OxyModifierKeys.None, PlotCommands.SnapTrack);
            plotController.BindMouseDown(OxyMouseButton.Right, OxyModifierKeys.None, PlotCommands.PanAt);
            // plotController.BindMouseEnter(PlotCommands.HoverTrack);

            return plotController;
        }


        private void HeatmapClickedCommand(IPlotView view, OxyMouseDownEventArgs args)
        {
            var viewXAxis = view.ActualModel.Axes.First(ax => ax.Tag == "X");
            var posFrame = viewXAxis.InverseTransform(args.Position.X);

            var viewYAxis = view.ActualModel.Axes.First(ax => ax.Tag == "Y");
            var posSensor = viewYAxis.InverseTransform(args.Position.Y);

            // SamplePoint dataPoint = new SamplePoint(posF)
            // regionFinder.Find(RegionType.VP, CurrentTimeFrameVM.Data, 

            logger.LogDebug($"Clicked pos {posFrame:F2}, {posSensor:F2}");
        }


        /// <summary>
        /// View의 TimeFrames listview 에서 선택된 item 객체를 받는다.
        /// </summary>
        /// <param name="selectedItem"></param>
        [RelayCommand]
        private void SelectionChanged(object selectedItem)
        {
            if (selectedItem == null) return;

            try
            {
                var timeFrameData = (selectedItem as TimeFrameViewModel).Data;
                CurrentTimeFrameVM = new TimeFrameViewModel(timeFrameData);
                MainPlotModel = CurrentTimeFrameVM.FramePlotModel;
                
                var heatmap = MainPlotModel.Series.OfType<HeatMapSeries>().FirstOrDefault();
                heatmap.TrackerFormatString = "{6:0.00}";
;                Observable.FromEvent<EventHandler<TrackerEventArgs>, TrackerEventArgs>(
                    handler => (sender, e) => handler(e),
                    handler => MainPlotModel.TrackerChanged += handler,
                    handler => MainPlotModel.TrackerChanged -= handler).Subscribe(HandleTrackerChanged);

                CurrentTimeFrameVM.Data.UpdateSensorBounds(timeFrameData.MinSensorBound, timeFrameData.MaxSensorBound);
                CurrentTimeFrameVM.RefreshPlotData();

                MainPlotController = BuildPlotcontroller();
                
                currentTimeFrameGraphVM = new TimeFrameGraphViewModel(timeFrameData);
                GraphPlotModel = currentTimeFrameGraphVM.FramePlotModel;
                
                currentTimeFrameGraphVM.Data.UpdateSensorBounds(timeFrameData.MinSensorBound, timeFrameData.MaxSensorBound);
                // currentTimeFrameGraphVM.RefreshPlotData();
                GraphPlotController = BuildPlotcontroller();
            }
            catch
            {
                logger.LogError("Selecting timeframe viewmodel got error");
            }
        }

        //TODO : SensorBoundsChanged와 같이 Time changed 에 대한 이벤트에 따른 mainmodel 업데이트 기능 필요

        private void SensorBoundsChanged(object recipient, SensorBoundsChangedMessage message)
        {
            if(CurrentTimeFrameVM != null)
            {
                CurrentTimeFrameVM.Data.UpdateSensorBounds(message.Value.MinBound, message.Value.MaxBound);
                CurrentTimeFrameVM.RefreshPlotData();

                currentTimeFrameGraphVM.Data.UpdateSensorBounds(message.Value.MinBound, message.Value.MaxBound);
                currentTimeFrameGraphVM.RefreshPlotData();
            }
        }

        private void HandleTrackerChanged(TrackerEventArgs args)
        {
            if (args.HitResult == null)
                return;
            
            // PlotView에서 Tracker 가 표시되지 않도록 한다
            MainPlotModel.PlotView.HideTracker();
            var datapoint = args.HitResult.DataPoint;
            StatusMessage = $"DataPoint : {datapoint.X}, {datapoint.Y}, value = {args.HitResult.Text}";
        }

        public TimeFrameGraphViewModel currentTimeFrameGraphVM { get; private set; }

        public TimeFrameViewModel CurrentTimeFrameVM { get; private set; }

        /// <summary>
        /// TimeFrame 리스트에서 이전 item을 선택한다.
        /// </summary>
        /// <param name="selectedIndex"></param>
        [RelayCommand]
        private void PreviousTimeFrame(int selectedIndex)
        {
            logger.LogTrace("Go to previous timeframe");
            if (selectedIndex > 0)
                SelectedIndexOfTimeFrameViewModel -= 1;
        }

        /// <summary>
        /// TimeFrame 리스트에서 다음 item을 선택한다.
        /// </summary>
        /// <param name="selectedIndex"></param>
        [RelayCommand]
        private void NextTimeFrame(int selectedIndex)
        {
            logger.LogTrace("Go to next timeframe");
            if (selectedIndex < TimeFrameViewModels.Count - 1)
                SelectedIndexOfTimeFrameViewModel += 1;
        }

        /// <summary>
        /// TimeFrame 에서의 데이터를 분석한다
        /// </summary>
        [RelayCommand]
        private void InspectFrame()
        {
            logger.LogTrace("Clicked");
        }

        /// <summary>
        /// 이전 TimeFrame에서 선택한 조건에 맞춰 모든 TimeFrame에 대해 자동으로 Inspect를 수행한다
        /// </summary>
        [RelayCommand]
        private void AutoInspectFrame()
        {
            logger.LogTrace("Clicked");
        }

        /// <summary>
        /// Inspect 결과를 초기화한다
        /// </summary>
        [RelayCommand]
        private void ResetInspect()
        {
            logger.LogTrace("Clicked");
        }

        /// <summary>
        /// Inspect를 Undo 한다
        /// </summary>
        [RelayCommand]
        private void UndoInspect()
        {
            logger.LogTrace("Clicked");
        }
    }
}