using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PlotCommands = OxyPlot.PlotCommands;
using Microsoft.Extensions.DependencyInjection;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.Services;
using SRHWiscMano.Core.Helpers;
using MahApps.Metro.Controls;
using OxyPlot.Annotations;
using System.Reflection;

namespace SRHWiscMano.Test
{
    [TestFixture, Apartment(ApartmentState.STA)]
    internal class TestOxyplotWindow : TestModelBase
    {
        private Window window = new Window();
        private PlotView plotView = new PlotView();

        [OneTimeSetUp]
        public void SetupOneTime()
        {
            Console.WriteLine($"{this.GetType().Namespace}");
        }

        [SetUp]
        public void SetUp()
        {
            window.Content = plotView;
            window.Closed += (sender, args) =>
            {
                // Stop the Dispatcher when the window is closed.
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            };
        }

        [TearDown]
        public void TearDown()
        {
            if (Environment.GetEnvironmentVariable("SKIP_STA_TEST") == "true")
            {
                Console.WriteLine("Skipping this test as per environment variable setting.");
                return;
                // Assert.Ignore("Skipping this test as per environment variable setting.");
            }

            window.Show();
            Dispatcher.Run();
        }


        /// <summary>
        /// Heatmap 을 그리며 Plot에 대한 모든 입력을 해제
        /// </summary>
        [Test]
        public void TestHeatmapSeries()
        {
            Console.WriteLine($"Called {this.GetType().Namespace}.{MethodBase.GetCurrentMethod().Name}");
            var model = CreateHeatmapPeaks(OxyPalettes.Hue64, true, 200);
            plotView.Model = model;
            var controller = new PlotController();
            controller.UnbindAll();
            plotView.Controller = controller;
        }

        /// <summary>
        /// PlotCommand Hover에 관한 명령을 테스트 한다.
        ///  - HoverPointsOnlyTrack : Data로 입력된 point 에 대해서만 track 을 수행한다.\
        ///  - HoverTrack : Plot 되는 데이터 모두에 대한 Track을 수행한다, data 사이의 interpolation 한 값을 얻을 수 있다.
        ///  - HoverSnapTrack : 가까운 Snap 된 포인트에 대해서 Track 정보를 표시한다.
        /// </summary>
        [Test]
        public void TestPlotCommandHover()
        {
            Console.WriteLine($"Called {this.GetType().Namespace}.{MethodBase.GetCurrentMethod().Name}");
            var model = CreateFuncionSeries();
            plotView.Model = model;
            var controller = new PlotController();
            controller.BindMouseEnter(PlotCommands.HoverPointsOnlyTrack); // Data로 입력된 point 에 대해서만 track 을 수행한다.
            // controller.BindMouseEnter(PlotCommands.HoverTrack); // Plot 되는 데이터 모두에 대한 Track을 수행한다, data 사이의 interpolation 한 값을 얻을 수 있다.
            // controller.BindMouseEnter(PlotCommands.HoverSnapTrack);  // 가까운 Snap 된 포인트에 대해서 Track 정보를 표시한다.
            plotView.Controller = controller;
        }



        public PlotModel CreateFuncionSeries()
        {
            var model = new PlotModel
            {
                Title = "Specified distance of the tracker fires",
                Subtitle = "Press the left mouse button to test the tracker.",
            };
            model.Series.Add(new FunctionSeries(Math.Sin, 0, 10, 100));
            return model;
        }

        [Test]
        public void TestPlotCommandPanning()
        {
            Console.WriteLine($"Called {this.GetType().Namespace}.{MethodBase.GetCurrentMethod().Name}");
            var model = ImportExamination();
            plotView.Model = model;

            plotView.Controller = WheelPanningController();

            // var controller = new PlotController();
            // controller.UnbindAll();
            // controller.BindMouseWheel(OxyModifierKeys.Control, PlotCommands.ZoomInAt);
            window.Loaded += Window_Loaded;
        }

        
        public PlotController WheelPanningController()
        {
            var controller = new PlotController();
            controller.UnbindAll();

            var handleWheel = new DelegatePlotCommand<OxyMouseWheelEventArgs>(
                (view, c, e) =>
                {
                    
                    e.Handled = true;
                    var dx = view.ActualModel.PlotArea.Width * 0.001 * e.Delta;
                    var dy = view.ActualModel.PlotArea.Height * 0.001 * e.Delta;
                    view.ActualModel.PanAllAxes(dx, 0);
                    view.InvalidatePlot(false);

                    // new PanManipulator(v)
                    // var m = new ZoomStepManipulator(v) { Step = e.Delta * 0.001 * 1, FineControl = e.IsControlDown };
                    // m.Started(e);

                    // controller.AddMouseManipulator(v, new PanManipulator(v), e);
                });

            
            controller.Bind(new OxyMouseWheelGesture(), handleWheel);
            controller.BindMouseWheel(OxyModifierKeys.Control, PlotCommands.ZoomWheel);

            return controller;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"{plotView.ActualWidth}, {plotView.ActualHeight}");
            var axesInfos = plotView.Model.Axes;//.Select()
            var xAxis = axesInfos.First(ax => ax.Tag == "X");
            // xAxis.Maximum = 20000;
            plotView.InvalidatePlot(false);
            // Debug.WriteLine({ plotView.Model.Axes[0].ActualMajorStep});
            // plotView.h
        }

        public PlotModel ImportExamination()
        {
            var services = new ServiceCollection();
            SRHWiscMano.Core.ServiceRegistration.ConfigureServices(services);
            var provider = services.BuildServiceProvider();

            var fileName = "100.txt";
            var examImporter = provider.GetService<IImportService<IExamination>>();
            var examData = examImporter.ReadFromFile(LoadTestData(fileName));

            var sensorCount = examData.SensorCount();
            var frameCount = examData.Samples.Count;

            var arrayData = new double[frameCount, sensorCount];
            for (int i = 0; i < frameCount; i++)
            {
                for (int j = 0; j < sensorCount; j++)
                {
                    arrayData[i, j] = examData.Samples[i].Values[j];
                }
            }

            // Create your heatmap series and add to MyModel
            var heatmapSeries = new HeatMapSeries
            {
                X0 = 0,
                X1 = frameCount - 1, // Assuming 28 sensors
                Y0 = 0,
                Y1 = sensorCount - 1,
                Data = arrayData /* Your 2D data array */,
                Interpolate = true
            };

            var model = new PlotModel { Title = "Peaks" };
            model.Series.Add(heatmapSeries);

            model.Axes.Add(new LinearColorAxis
            {
                Position = AxisPosition.Right, 
                Palette = OxyPalettes.Rainbow(200), 
                HighColor = OxyColors.Gray,
                LowColor = OxyColors.Gray,
                
            });

            model.Axes.Add(new LinearAxis()
            {
                IsPanEnabled = false,
                IsZoomEnabled = false,
                Position = AxisPosition.Left,
                MaximumPadding = 0,
                MinimumPadding = 0,
                StartPosition = 1,
                EndPosition = 0,
                Minimum = 0,                // 초기 시작값
                Maximum = sensorCount - 1,  // 초기 최대값
                AbsoluteMinimum = 0,        // Panning 최소값
                AbsoluteMaximum = sensorCount - 1,  // Panning 최대값
                Tag = "Y"
            });


            // X-Axis
            model.Axes.Add(new LinearAxis()
            {
                // IsZoomEnabled = false,
                Position = AxisPosition.Bottom,
                MinimumPadding = 0,
                Minimum = 0,
                Maximum = 3000,// - 1,
                MajorStep = 100,
                AbsoluteMinimum = 0,
                AbsoluteMaximum = frameCount - 1,
                MinorStep = frameCount - 1, // 최대 범위를 입력하여 MinorStep 이 표시되지 않도록 한다
                Tag = "X"
            });
            return model;
        }


        public static PlotModel CreateHeatmapPeaks(OxyPalette palette = null, bool includeContours = true, int n = 100)
        {
            double x0 = -3.1;
            double x1 = 3.1;
            double y0 = -3;
            double y1 = 3;
            Func<double, double, double> peaks = (x, y) =>
                3 * (1 - x) * (1 - x) * Math.Exp(-(x * x) - (y + 1) * (y + 1)) -
                10 * (x / 5 - x * x * x - y * y * y * y * y) * Math.Exp(-x * x - y * y) -
                1.0 / 3 * Math.Exp(-(x + 1) * (x + 1) - y * y);
            var xvalues = ArrayBuilder.CreateVector(x0, x1, n);
            var yvalues = ArrayBuilder.CreateVector(y0, y1, n);
            var peaksData = ArrayBuilder.Evaluate(peaks, xvalues, yvalues);

            var model = new PlotModel { Title = "Peaks" };
            model.Axes.Add(new LinearColorAxis
            {
                Position = AxisPosition.Right, Palette = palette ?? OxyPalettes.Jet(500), HighColor = OxyColors.Gray,
                LowColor = OxyColors.Black
            });

            var hms = new HeatMapSeries { X0 = x0, X1 = x1, Y0 = y0, Y1 = y1, Data = peaksData };
            model.Series.Add(hms);
            if (includeContours)
            {
                var cs = new ContourSeries
                {
                    Color = OxyColors.Black,
                    FontSize = 0,
                    ContourLevelStep = 1,
                    LabelBackground = OxyColors.Undefined,
                    ColumnCoordinates = yvalues,
                    RowCoordinates = xvalues,
                    Data = peaksData
                };
                model.Series.Add(cs);
            }

            return model;
        }
    }
}