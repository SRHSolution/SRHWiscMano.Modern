using NodaTime;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.Models.Results;

namespace SRHWiscMano.Core.Services.Report
{
    /// <summary>
    /// Result를 위한 계산식
    /// </summary>
    public static class RegionCalculator
    {
        public static SwallowResults<double> CalcResults(ITimeFrame state)
        {
            SwallowResults<double> currentResults = new SwallowResults<double>();
            currentResults.VP = CalcStats(state, RegionType.VP);

            currentResults.TB = CalcStats(state, RegionType.TB);
            currentResults.HP = CalcStats(state, RegionType.HP);

            currentResults.PreUES = CalcStats(state, RegionType.PreUES);
            currentResults.PostUES = CalcStats(state, RegionType.PostUES);
            currentResults.UES = CalcUesStats(state, RegionType.UES);
            currentResults.MaxPressures = MaxPressuresCalculator.Calc(state).ToList();
            currentResults.PressureAtVPMax = PressureAtVPMaxCalculator.Calc(state).ToList();
            currentResults.PressureAtTBMax = PressureAtTBMaxCalculator.Calc(state).ToList();
            currentResults.PressureGradient = PressureGradientCalculator.CalcDefaultRange(state).ToList();
            currentResults.TotalSwallowDuration = OverallParamsCalculator.CalcTotalDuration(state).ToTotalSeconds();
            currentResults.TotalPharyngealPressure =
                OverallParamsCalculator.CalcTotalPharyngealPressure(state, currentResults);
            return currentResults;
        }

        private static ResultParameters<double> CalcStats(
            ITimeFrame state,
            RegionType type)
        {
            IRegion region = state.GetRegion(type);
            ResultParameters<double> regionStats = new ResultParameters<double>();
            regionStats.Duration = region.TimeRange.Duration.ToTotalSeconds();
            CalculateMinMaxAndIntegrals(state, region).FillRegionStats(regionStats);
            return regionStats;
        }

        private static UesResultParameters<double> CalcUesStats(
            ITimeFrame state,
            RegionType type)
        {
            IRegion region = state.GetRegion(type);
            UesResultParameters<double> regionStats = new UesResultParameters<double>();
            regionStats.NadirDuration = UesNadirCalculator.CalcDuration(state, region).ToTotalSeconds();
            regionStats.Duration = region.TimeRange.Duration.ToTotalSeconds();
            CalculateMinMaxAndIntegrals(state, region).FillUesRegionStats(regionStats);
            regionStats.MinimumPressure = UesNadirCalculator.CalcMinimum(state, region);
            return regionStats;
        }

        private static MinMaxAndIntegrals CalculateMinMaxAndIntegrals(
            ITimeFrame state,
            IRegion region)
        {
            double maximum = double.MinValue;
            double minimum = double.MaxValue;
            double integralUnderMaximum = 0.0;
            double totalIntegral = 0.0;
            IEnumerable<TimeSample> samples = state.ExamData.Samples.SamplesInTimeRange(region.TimeRange);
            if (!samples.Any())
                throw new ResultsCalculationException(string.Format(
                    "No sample in time range for {0} region of snapshot {1}", region.Type.ToString(), state.Id));
            for (int i = region.SensorRange.Start; i < region.SensorRange.End; ++i)
            {
                double num1 = samples.SmoothMax(i);
                double num2 = samples.SmoothMin(i);
                double num3 = samples.TrapezoidalIntegral(s => s.TimeInSeconds, s => s.Values[i]);
                if (num1 > maximum)
                {
                    maximum = num1;
                    integralUnderMaximum = num3;
                }

                if (num2 < minimum)
                    minimum = num2;
                totalIntegral += num3;
            }

            return new MinMaxAndIntegrals(maximum, minimum, integralUnderMaximum, totalIntegral);
        }

        private static double TrapezoidalIntegral<T>(
            this IEnumerable<T> values,
            Func<T, double> x,
            Func<T, double> fx)
        {
            return values.Pairwise().Select(p => TrapezoidalApprox(x(p.Item1), x(p.Item2), fx(p.Item1), fx(p.Item2)))
                .Sum();
        }

        private static double TrapezoidalApprox(double a, double b, double fa, double fb)
        {
            return (b - a) * ((fa + fb) / 2.0);
        }

        private static T FillRegionStats<T>(
            this MinMaxAndIntegrals calcResults,
            T regionStats)
            where T : ResultParameters<double>
        {
            regionStats.MaximumPressure = calcResults.Maximum;
            regionStats.TotalPressureFromMaxSensor = calcResults.IntegralUnderMaximum;
            regionStats.TotalVolumePressure = calcResults.TotalIntegral;
            return regionStats;
        }

        private static UesResultParameters<double> FillUesRegionStats(
            this MinMaxAndIntegrals calcResults,
            UesResultParameters<double> regionStats)
        {
            calcResults.FillRegionStats(regionStats);
            regionStats.MinimumPressure = calcResults.Minimum;
            return regionStats;
        }

        private static double ToTotalSeconds(this Duration duration)
        {
            return duration.ToTimeSpan().TotalSeconds;
        }

        private class MinMaxAndIntegrals
        {
            public MinMaxAndIntegrals(
                double maximum,
                double minimum,
                double integralUnderMaximum,
                double totalIntegral)
            {
                Maximum = maximum;
                Minimum = minimum;
                IntegralUnderMaximum = integralUnderMaximum;
                TotalIntegral = totalIntegral;
            }

            public double Maximum { get; }

            public double Minimum { get; }

            public double IntegralUnderMaximum { get; }

            public double TotalIntegral { get; }
        }
    }
}