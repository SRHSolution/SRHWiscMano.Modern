using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using SRHWiscMano.App.Services;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.App.ViewModels
{
    public partial class ReportViewModel : ViewModelBase, IReportViewModel
    {
        private readonly ILogger<ReportViewModel> logger;
        private readonly SharedService sharedService;

        [ObservableProperty] private PlotModel pressureModel;
        [ObservableProperty] private PlotController pressureModelController;

        [ObservableProperty] private PlotModel pressureModelAtVP;
        [ObservableProperty] private PlotController pressureModelAtVPController;
        [ObservableProperty] private PlotModel pressureModelAtTB;
        [ObservableProperty] private PlotController pressureModelAtTBController;
        [ObservableProperty] private PlotModel pressureGradientModel;
        [ObservableProperty] private PlotController pressureGradientController;

        /// <summary>
        /// Designer Datacontext를 위한 생성자
        /// </summary>
        public ReportViewModel()
        {
        }

        public ReportViewModel(ILogger<ReportViewModel> logger, SharedService sharedService)
        {
            this.logger = logger;
            this.sharedService = sharedService;
            PressureModel = CreatePressurePlot("Pressure Maximum");
            PressureModelAtVP = CreatePressurePlot("Pressure Maximum at VP");
            PressureModelAtTB = CreatePressurePlot("Pressure Maximum at TB");
        }

        private PlotModel CreatePressurePlot(string title)
        {
            var plotModel = new PlotModel();
            plotModel.Title = title;
            plotModel.Series.Add(new LineSeries()
            {

            });

            AddAxes(plotModel);
            plotModel.InvalidatePlot(true);
            return plotModel;
        }

        private void AddAxes(PlotModel model)
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
            xAxis.ActualLabels.Add("VP");
            xAxis.ActualLabels.Add("VP");
            xAxis.ActualLabels.Add("VP");
            xAxis.ActualLabels.Add("TB");
            xAxis.ActualLabels.Add("TB");
            xAxis.ActualLabels.Add("TB");
            xAxis.ActualLabels.Add("HP");
            xAxis.ActualLabels.Add("HP");
            xAxis.ActualLabels.Add("UES");
            xAxis.ActualLabels.Add("UES");
            xAxis.ActualLabels.Add("UES");
            xAxis.ActualLabels.Add("UES");
            xAxis.ActualLabels.Add("UES");

            xAxis.Labels.Add("VP");
            xAxis.Labels.Add("VP");
            xAxis.Labels.Add("VP");
            xAxis.Labels.Add("TB");
            xAxis.Labels.Add("TB");
            xAxis.Labels.Add("TB");
            xAxis.Labels.Add("HP");
            xAxis.Labels.Add("HP");
            xAxis.Labels.Add("UES");
            xAxis.Labels.Add("UES");
            xAxis.Labels.Add("UES");
            xAxis.Labels.Add("UES");
            xAxis.Labels.Add("UES");

            model.Axes.Add(xAxis);


        }

    }
}
