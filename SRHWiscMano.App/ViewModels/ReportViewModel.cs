using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xaml;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using MoreLinq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using SRHWiscMano.App.Services;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models.Results;
using SRHWiscMano.Core.Services;
using SRHWiscMano.Core.Services.Report;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.App.ViewModels
{
    public partial class ReportViewModel : ViewModelBase, IReportViewModel
    {
        private readonly ILogger<ReportViewModel> logger;
        private readonly SharedService sharedService;
        private readonly IResultsCalculator calculator;
        private readonly IExportService<ExamResults<OutlierResult>> exportService;

        [ObservableProperty] private PlotModel modelPressureMax;
        [ObservableProperty] private PlotController cntrPressureMax;

        [ObservableProperty] private PlotModel modelPressureMaxAtVP;
        [ObservableProperty] private PlotController cntrPressureMaxAtVP;
        [ObservableProperty] private PlotModel modelPressureMaxAtTB;
        [ObservableProperty] private PlotController cntrPressureMaxAtTB;
        [ObservableProperty] private PlotModel modelPressureGradient;
        [ObservableProperty] private PlotController cntrPressureGradient;

        [ObservableProperty] private ExamResults<OutlierResult> examResult;

        /// <summary>
        /// Designer Datacontext를 위한 생성자
        /// </summary>
        public ReportViewModel()
        {
        }

        public ReportViewModel(ILogger<ReportViewModel> logger, SharedService sharedService, IResultsCalculator calculator, IExportService<ExamResults<OutlierResult>> exportService)
        {
            this.logger = logger;
            this.sharedService = sharedService;
            this.calculator = calculator;
            this.exportService = exportService;
            ModelPressureMax = CreatePlotForPressureMax("Pressure Maximum");
            ModelPressureMaxAtVP = CreatePlotForPressureMax("Pressure Maximum at VP");
            ModelPressureMaxAtTB = CreatePlotForPressureMax("Pressure Maximum at TB");
            ModelPressureGradient = CreatePlotForPressureGradient("Pressure Gradient");

            // DummyData();
        }

        /// <summary>
        /// Report view가 Loaded 되었을 때 실행된다
        /// </summary>
        [RelayCommand]
        private void NavigatedFrom()
        {
            logger.LogDebug("Report view is loaded");
            var selectedTimeFrames = sharedService.TimeFrames.KeyValues.Where(kv => kv.Value.IsSelected && kv.Value.AllRegionsAreDefined()).ToList();

            if (selectedTimeFrames.Count == 0)
                return;

            ExamResult = calculator.CalculateTimeframeResults(selectedTimeFrames.Select(kv => kv.Value));
            
            UpdateExamResultOnPlotModels();
        }

        private void UpdateExamResultOnPlotModels()
        {
            if (ExamResult.Individuals.Count == 0)
                return;

            AddResultDataToPlotModel(ModelPressureMax, ExamResult.Aggregate.MaxPressures);
            AddResultDataToPlotModel(ModelPressureMaxAtVP, ExamResult.Aggregate.PressureAtVPMax);
            AddResultDataToPlotModel(ModelPressureMaxAtTB, ExamResult.Aggregate.PressureAtTBMax);
        }

        private void AddResultDataToPlotModel(PlotModel model, IEnumerable<MeanAndDeviation> result)
        {
            var lineMean = new LineSeries();
            lineMean.LineStyle = LineStyle.Solid;
            lineMean.Color = OxyColors.DarkGray;
            lineMean.StrokeThickness = 2;
            var lineStdUpper = new LineSeries();
            lineStdUpper.Color = OxyColors.LightBlue;
            lineStdUpper.StrokeThickness = 1;
            var lineStdLower = new LineSeries();
            lineStdUpper.Color = OxyColors.LightBlue;
            lineStdUpper.StrokeThickness = 1;

            result.SelectWithIndex().ForEach(si =>
            {
                lineMean.Points.Add(new DataPoint(si.Item2, si.Item1.Mean));
                lineStdUpper.Points.Add(new DataPoint(si.Item2, si.Item1.Mean + si.Item1.StandardDeviation));
                lineStdLower.Points.Add(new DataPoint(si.Item2, si.Item1.Mean - si.Item1.StandardDeviation));
            });

            model.Series.Clear();
            model.Series.Add(lineMean);
            model.Series.Add(lineStdUpper);
            model.Series.Add(lineStdLower);
            model.InvalidatePlot(true);
        }
        

        private PlotModel CreatePlotForPressureMax(string title)
        {
            var plotModel = new PlotModel();
            plotModel.Title = title;

            AddAxesForPressureMax(plotModel);
            plotModel.InvalidatePlot(true);
            return plotModel;
        }

        private void AddAxesForPressureMax(PlotModel model)
        {
            foreach (var axis in model.Axes)
            {
                model.Axes.Remove(axis);
            }

            // Y-Axis
            model.Axes.Add(new LinearAxis()
            {
                IsPanEnabled = false,
                IsZoomEnabled = false,
                Position = AxisPosition.Left,
                MaximumPadding = 0,
                MinimumPadding = 0,
                MajorStep = 50,
                
                // Minimum = 0, // 초기 시작값
                // Maximum = ((ySize + 1)), // 초기 최대값
                // AbsoluteMinimum = -0, // Panning 최소값
                // AbsoluteMaximum = ((ySize + 1)), // Panning 최대값, LineSeries는 데이터를 한 step shift 하여 표시하기 위해 ySize+1 한다
                IsAxisVisible = true,
                Tag = "Y"
            });
            // LinearAxis 생성 및 설정 (X 축)
            var linearXAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "X Axis",
                MajorStep = 1,
                MinorStep = 10,
                Minimum = 0,
                Maximum = 9,
                AbsoluteMinimum = 0,
                AbsoluteMaximum = 9,
            };

            var customLabels = new Dictionary<double, string>
            {
                { 0, "VP" },
                { 1, "VP" },
                { 2, "VP" },
                // { 3, "TB" },
                { 3, "TB" },
                { 4, "HP" },
                // { 6, "HP" },
                { 5, "UES" },
                { 6, "UES" },
                { 7, "UES" },
                { 8, "UES" },
                { 9, "UES" },
            };

            foreach (var label in customLabels)
            {
                linearXAxis.AxisTickToLabelDistance = 10;
                linearXAxis.LabelFormatter = d =>
                {
                    return customLabels.ContainsKey(d) ? customLabels[d] : string.Empty;
                };
            }

            model.Axes.Add(linearXAxis);

        }

        private PlotModel CreatePlotForPressureGradient(string title)
        {
            var plotModel = new PlotModel();
            plotModel.Title = title;
            plotModel.Series.Add(new LineSeries()
            {

            });

            AddAxesForPressureGradient(plotModel);
            plotModel.InvalidatePlot(true);
            return plotModel;
        }

        private void AddAxesForPressureGradient(PlotModel model)
        {
            foreach (var axis in model.Axes)
            {
                model.Axes.Remove(axis);
            }

            // Y-Axis
            model.Axes.Add(new LinearAxis()
            {
                IsPanEnabled = false,
                IsZoomEnabled = false,
                Position = AxisPosition.Left,
                MaximumPadding = 0,
                MinimumPadding = 0,

                // Minimum = 0, // 초기 시작값
                // Maximum = ((ySize + 1)), // 초기 최대값
                // AbsoluteMinimum = -0, // Panning 최소값
                // AbsoluteMaximum = ((ySize + 1)), // Panning 최대값, LineSeries는 데이터를 한 step shift 하여 표시하기 위해 ySize+1 한다
                IsAxisVisible = true,
                Tag = "Y"
            });

            // X-Axis
            var xAxis = new CategoryAxis()
            {
                IsZoomEnabled = false,
                IsPanEnabled = false,
                // LabelFormatter = value => $"{value / 100}",
                Position = AxisPosition.Bottom,
                MinimumPadding = 0,
                // Minimum = 0,
                // Maximum = xSize - 1,
                // MajorStep = xSize,
                // MinorStep = xSize, // 최대 범위를 입력하여 MinorStep 이 표시되지 않도록 한다
                // AbsoluteMinimum = 0,
                // AbsoluteMaximum = xSize - 1,
                IsAxisVisible = true,
                Tag = "X"
            };
        }
        
        [RelayCommand]
        private void ExportToCSV()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            // 파일 필터를 설정합니다.
            sfd.Filter = "CSV files (*.csv)|*.csv";
            sfd.Title = "Save a Text File";
            sfd.FileName = $"Report_{DateTime.Now:dd-MM-yy}";
            sfd.DefaultExt = "csv";
            sfd.AddExtension = true;

            // 다이얼로그를 표시하고, 사용자가 파일을 선택했는지 확인합니다.
            if (sfd.ShowDialog() == true)
            {
                // 선택한 파일 경로를 가져옵니다.
                string filePath = sfd.FileName;

                // 파일에 데이터를 씁니다.
                exportService.WriteToFile(ExamResult, filePath);

                // 사용자에게 저장 완료 메시지를 표시합니다.
                MessageBox.Show("File saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
