﻿using System;
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
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoreLinq.Extensions;
using NLog;
using NodaTime;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using SRHWiscMano.App.Data;
using SRHWiscMano.App.Services;
using SRHWiscMano.Core.Data;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.Services.Detection;
using SRHWiscMano.Core.Services.Diagnostics;
using SRHWiscMano.Core.ViewModels;


namespace SRHWiscMano.App.ViewModels
{
    public partial class AnalyzerViewModel : ViewModelBase, IAnalyzerViewModel
    {
        #region Fields & Properties

        private readonly ILogger<AnalyzerViewModel> logger;
        private readonly SharedService sharedService;
        private readonly IOptions<AppSettings> settings;
        private readonly IRegionFinder regionFinder;
        private readonly SourceCache<ITimeFrame, int> timeFrames;

        [ObservableProperty] private PlotModel mainPlotModel;
        [ObservableProperty] private PlotController mainPlotController;

        [ObservableProperty] private PlotModel graphPlotModel;
        [ObservableProperty] private PlotController graphPlotController;


        /// <summary>
        /// 현재 선택된 TimeFrame ViewModel 원본 데이터
        /// </summary>
        public TimeFrameViewModel CurrentTimeFrameVM { get; private set; }

        /// <summary>
        /// 현재 선택되어 표시되고 있는 TimeFrame Heatmap ViewModel
        /// </summary>
        public TimeFrameViewModel CurrentTimeFrameHeatmapVM { get; private set; }

        /// <summary>
        /// 현재 선택되어 표시되고 있는 TimeFrame Lineseries ViewModel
        /// </summary>
        public TimeFrameGraphViewModel CurrentTimeFrameGraphVM { get; private set; }

        [ObservableProperty] private string statusMessage = "Test status message";

        [ObservableProperty] private int selectedIndexOfTimeFrameViewModel = -1;

        private DetectionDiagnostics diagnostics;

        /// <summary>
        /// View에서 표시할 전체 TimeFrameViewModels 
        /// </summary>
        public ObservableCollection<TimeFrameViewModel> TimeFrameViewModels { get; } = new();

        /// <summary>
        /// Heatmap clicked 이벤트에 대한 delegate를 등록하는 변수
        /// </summary>
        public DelegatePlotCommand<OxyMouseDownEventArgs> HeatmapClicked { get; set; }

        public DelegatePlotCommand<OxyMouseDownEventArgs> GraphClicked { get; set; }

        #endregion

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
            timeFrames.Connect().Subscribe(HandleBindingTimeFrames);

            WeakReferenceMessenger.Default.Register<SensorBoundsChangedMessage>(this, SensorBoundsChanged);
        }

        /// <summary>
        /// SharedService의 TimeFrames에 등록된 데이터를 View에 binding 작업을 수행한다.
        /// </summary>
        /// <param name="changeSet"></param>
        private void HandleBindingTimeFrames(IChangeSet<ITimeFrame, int> changeSet)
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

                    // Binding 된 TimeFrame의 Time/Sensor Bound를 변경될 시에 각각에 해당하는 viewmodel을 update한다.
                    case ChangeReason.Update:
                        var updItem = TimeFrameViewModels.SingleOrDefault(item => item.Id == change.Current.Id);
                        updItem.RefreshPlotData();

                        if (CurrentTimeFrameHeatmapVM?.Id == change.Current.Id)
                        {
                            CurrentTimeFrameHeatmapVM.RefreshPlotData();
                            CurrentTimeFrameGraphVM.RefreshPlotData();
                        }

                        break;

                    case ChangeReason.Refresh:

                        break;
                }
            }
        }

        /// <summary>
        /// TimeFrameViewModel 이 View에서 load 되었을 때의 이벤트 처리
        /// 처음에만 한번 0으로 변경한다
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
        /// View의 TimeFrames listview 에서 선택된 item 객체를 받는다.
        /// </summary>
        /// <param name="selectedItem"></param>
        [RelayCommand]
        private void SelectionChanged(object selectedItem)
        {
            if (selectedItem == null) return;

            try
            {
                CurrentTimeFrameVM = (selectedItem as TimeFrameViewModel);
                CurrentTimeFrameHeatmapVM = new TimeFrameViewModel(CurrentTimeFrameVM.Data);
                MainPlotModel = CurrentTimeFrameHeatmapVM.FramePlotModel;

                var heatmap = MainPlotModel.Series.OfType<HeatMapSeries>().FirstOrDefault();
                heatmap.TrackerFormatString = "{6:0.00}";
                ;
                Observable.FromEvent<EventHandler<TrackerEventArgs>, TrackerEventArgs>(
                    handler => (sender, e) => handler(e),
                    handler => MainPlotModel.TrackerChanged += handler,
                    handler => MainPlotModel.TrackerChanged -= handler).Subscribe(HandleTrackerChanged);

                MainPlotController = BuildMainPlotcontroller();
                CurrentTimeFrameHeatmapVM.RefreshPlotData();

                CurrentTimeFrameGraphVM = new TimeFrameGraphViewModel(CurrentTimeFrameVM.Data);
                GraphPlotModel = CurrentTimeFrameGraphVM.FramePlotModel;
                GraphPlotController = BuildGraphPlotcontroller();
                CurrentTimeFrameGraphVM.RefreshPlotData();
            }
            catch
            {
                logger.LogError("Selecting timeframe viewmodel got error");
            }
        }

        /// <summary>
        /// Sensor Bound 변경에 다른 이벤트 처리
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="message"></param>
        private void SensorBoundsChanged(object recipient, SensorBoundsChangedMessage message)
        {
            if (CurrentTimeFrameHeatmapVM != null)
            {
                CurrentTimeFrameHeatmapVM.Data.UpdateSensorBounds(message.Value.MinBound, message.Value.MaxBound);
                CurrentTimeFrameHeatmapVM.RefreshPlotData();

                CurrentTimeFrameGraphVM.Data.UpdateSensorBounds(message.Value.MinBound, message.Value.MaxBound);
                CurrentTimeFrameGraphVM.RefreshPlotData();
            }
        }

        /// <summary>
        /// MainPlot을 위한 Controller 구성
        /// </summary>
        /// <returns></returns>
        private PlotController BuildMainPlotcontroller()
        {
            var plotController = new PlotController();
            // plotController.UnbindAll();

            // Custom Command 함수를 위한 Delegate를 정의한다.
            HeatmapClicked =
                new DelegatePlotCommand<OxyMouseDownEventArgs>((view, controller, args) =>
                    HeatmapClickedCommand(view, args));

            // Custom command의 delegate를 이벤트에 bind 한다
            plotController.BindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Control, HeatmapClicked);
            plotController.BindMouseDown(OxyMouseButton.Left, OxyModifierKeys.None, PlotCommands.SnapTrack);

            return plotController;
        }

        /// <summary>
        /// GraphPlot을 위한 Controller 구성
        /// </summary>
        /// <returns></returns>
        private PlotController BuildGraphPlotcontroller()
        {
            var plotController = new PlotController();
            // plotController.UnbindAll();
            GraphClicked =
                new DelegatePlotCommand<OxyMouseDownEventArgs>((view, controller, args) =>
                    GraphClickedCommand(view, args));

            plotController.BindMouseDown(OxyMouseButton.Left, OxyModifierKeys.None, GraphClicked);
            plotController.BindMouseDown(OxyMouseButton.Left, OxyModifierKeys.None, PlotCommands.SnapTrack);
            // plotController.BindMouseDown(OxyMouseButton.Right, OxyModifierKeys.None, PlotCommands.PanAt);
            // plotController.BindMouseEnter(PlotCommands.HoverTrack);

            return plotController;
        }

        /// <summary>
        /// Heatmap 에서 Tracker 변경 이벤트처리
        /// </summary>
        /// <param name="args"></param>
        private void HandleTrackerChanged(TrackerEventArgs args)
        {
            if (args.HitResult == null)
                return;

            // PlotView에서 Tracker 가 표시되지 않도록 한다
            MainPlotModel.PlotView.HideTracker();
            var datapoint = args.HitResult.DataPoint;

            var viewXAxis = MainPlotModel.PlotView.ActualModel.Axes.First(ax => ax.Tag == "X");
            var posX = viewXAxis.InverseTransform(datapoint.X);

            // 현재의 TimeFrame에서 Pick 한 위치의 실제 Instant time을 계산한다
            var pickPosX = Duration.FromMilliseconds((long) (datapoint.X * 10 + 0.5));
            var posTime = CurrentTimeFrameHeatmapVM.Data.TimeRange().Start.Plus(pickPosX);

            // 현재의 Sensor Range를 기준으로 센서 몇번에서 클릭되었는지를 계산한다
            var tmpPos = (int) (datapoint.Y / CurrentTimeFrameHeatmapVM.Data.ExamData.InterpolationScale + 0.5);
            // var posSensor = CurrentTimeFrameHeatmapVM.Data.SensorRange().Lesser + tmpPos;
            var posSensor = tmpPos;

            StatusMessage =
                $"DataPoint: {datapoint.X}, {datapoint.Y} => {(double) posTime.ToMillisecondsFromEpoch() / 1000:F2} sec, sensor : {posSensor}, value : {args.HitResult.Text}";
        }


        private void HeatmapClickedCommand(IPlotView view, OxyMouseDownEventArgs args)
        {
            var viewXAxis = view.ActualModel.Axes.First(ax => ax.Tag == "X");
            var posX = viewXAxis.InverseTransform(args.Position.X);

            var viewYAxis = view.ActualModel.Axes.First(ax => ax.Tag == "Y");
            var posY = viewYAxis.InverseTransform(args.Position.Y);

            // 현재의 TimeFrame에서 Pick 한 위치의 실제 Instant time을 계산한다
            var pickPosX = Duration.FromMilliseconds((long) (posX * 10));
            var posTime = CurrentTimeFrameHeatmapVM.Data.TimeRange().Start.Plus(pickPosX);

            // 현재의 Sensor Range를 기준으로 센서 몇번에서 클릭되었는지를 계산한다
            var tmpPos = (int) (posY / CurrentTimeFrameHeatmapVM.Data.ExamData.InterpolationScale + 0.5);
            // var posSensor = CurrentTimeFrameHeatmapVM.Data.SensorRange().Lesser + tmpPos;
            var posSensor = tmpPos;

            logger.LogTrace($"Clicked pos {posX:F2}, {posY:F2}");
            logger.LogTrace(
                $"Frame pos {CurrentTimeFrameHeatmapVM.Time.ToUnixTimeMilliseconds()} {CurrentTimeFrameHeatmapVM.Data.TimeRange().Start.ToUnixTimeMilliseconds():F2}, {CurrentTimeFrameHeatmapVM.Data.TimeRange().End.ToUnixTimeMilliseconds():F2}");
            logger.LogTrace($"Pick Sample : {(double) posTime.ToUnixTimeMilliseconds() / 1000} msec, {posSensor}");

            SamplePoint pickPoint = new SamplePoint(posTime, (int) posSensor);
            FindRegions(pickPoint);
            TryAutoFindRegions(pickPoint);
        }


        private void FindRegions(SamplePoint clickPoint)
        {
            RegionSelectStep step = CurrentTimeFrameVM.RegionSelectSteps.FirstOrDefault(s => !s.IsCompleted);

            if (step == null && CurrentTimeFrameVM.AllStepsAreCompleted)
                return;
            try
            {
                var region = regionFinder.Find(step.Type, CurrentTimeFrameHeatmapVM.Data, clickPoint,
                    RegionFinderConfig.Default,
                    Diagnostics);

                if (region.SensorRange.Greater > CurrentTimeFrameHeatmapVM.Data.SensorRange().Greater)
                    throw new RegionFinderException("Selected TopSensor is too low.");
                if (region.SensorRange.Lesser < CurrentTimeFrameHeatmapVM.Data.SensorRange().Lesser + 1)
                    throw new RegionFinderException("Selected BottomSensor is too high.");
                if (!region.SensorRange.IsForward)
                {
                    logger.LogError($"Sensor range backwards {region.SensorRange.Start} -> {region.SensorRange.End}");
                    throw new RegionFinderException("Error calculating region.");
                }

                if (region != null)
                {
                    CurrentTimeFrameVM.Data.Regions.Add(region);
                }

                logger.LogTrace(region.ToString());

                // DrawRegionBox(view, region);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }

        private void TryAutoFindRegions(SamplePoint pickPoint)
        {
            var cpltRegions = CurrentTimeFrameVM.Data.Regions.Items;

            // 기본 조건으로 VP, PreUES, PostUES를 완료해야한다
            if (cpltRegions.Count() < 3)
                return;
            var check = cpltRegions.Any(s => s.Type == RegionType.VP) &&
                        cpltRegions.Any(s => s.Type == RegionType.PreUES) &&
                        cpltRegions.Any(s => s.Type == RegionType.PostUES);

            if (!check)
                return;

            if (!TryAutoAddRegions(RegionType.UES) || !TryAutoAddRegions(RegionType.TB) ||
                !TryAutoAddRegions(RegionType.HP))
                return;
        }

        private bool TryAutoAddRegions(RegionType stepType)
        {
            var cpltRegions = CurrentTimeFrameVM.Data.Regions.Items;
            if (!cpltRegions.Any(s => s.Type == stepType))
            {
                var region = regionFinder.Find(stepType, CurrentTimeFrameHeatmapVM.Data, null, RegionFinderConfig.Default, Diagnostics);
                if (region != null)
                {
                    CurrentTimeFrameVM.Data.Regions.Add(region);
                    return true;
                }

                return false;
            }
            return true;
        }

        private void DrawRegionBox(IPlotView view, Region region)
        {
            var rangeXStart = region.TimeRange.Start.Minus(CurrentTimeFrameHeatmapVM.Data.TimeRange().Start)
                .TotalMilliseconds / 10;
            var rangeXEnd = region.TimeRange.End.Minus(CurrentTimeFrameHeatmapVM.Data.TimeRange().Start)
                                .TotalMilliseconds /
                            10;

            var rangeYTop = region.SensorRange.Greater * CurrentTimeFrameHeatmapVM.Data.ExamData.InterpolationScale;

            var rangeYBottom = region.SensorRange.Lesser * CurrentTimeFrameHeatmapVM.Data.ExamData.InterpolationScale;
            var regColor = RegionSelectStep.GetStandardSteps(null).Where(stp => stp.Type == region.Type)
                .Select(stp => stp.Color).FirstOrDefault(Colors.Black);

            var rectangleAnnotation = new RectangleAnnotation
            {
                MinimumX = rangeXStart,
                MaximumX = rangeXEnd,
                MinimumY = rangeYTop,
                MaximumY = rangeYBottom,
                Fill = OxyColors.Transparent,
                Stroke = OxyColor.FromArgb(regColor.A, regColor.R, regColor.G, regColor.B),
                StrokeThickness = 2
            };
            view.ActualModel.Annotations.Add(rectangleAnnotation);
            view.ActualModel.InvalidatePlot(false);
        }

        private DetectionDiagnostics Diagnostics
        {
            get
            {
                if (diagnostics == null)
                    diagnostics = new DetectionDiagnostics(CurrentTimeFrameVM.Data.ExamData, CurrentTimeFrameVM.Data,
                        Duration.FromSeconds(3L));
                return diagnostics;
            }
        }

        private void GraphClickedCommand(IPlotView view, OxyMouseDownEventArgs args)
        {
            var viewXAxis = view.ActualModel.Axes.First(ax => ax.Tag == "X");
            var posFrame = viewXAxis.InverseTransform(args.Position.X);

            var viewYAxis = view.ActualModel.Axes.First(ax => ax.Tag == "Y");
            var posSensor = viewYAxis.InverseTransform(args.Position.Y);

            logger.LogDebug($"Clicked pos {posFrame:F2}, {posSensor:F2}");
        }


        #region 버튼 이벤트

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
            CurrentTimeFrameVM.Data.Regions.Clear();
        }

        /// <summary>
        /// Inspect를 Undo 한다
        /// </summary>
        [RelayCommand]
        private void UndoInspect()
        {
            logger.LogTrace("Clicked");
        }

        #endregion
    }
}