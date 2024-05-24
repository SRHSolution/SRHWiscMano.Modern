using MathNet.Numerics.Interpolation;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.Services.Report
{
    internal static class RegionInterpolation
    {
        /// <summary>
        /// Interpolation List
        /// </summary>
        private static readonly Tuple<RegionType, int>[] SectionsUsingMP = new Tuple<RegionType, int>[3]
        {
            Tuple.Create(RegionType.VP, 6),
            Tuple.Create(RegionType.MP, 8),
            Tuple.Create(RegionType.UES, 10)
        };

        /// <summary>
        /// Interpolation List
        /// </summary>
        private static readonly Tuple<RegionType, int>[] SectionsUsingTBAndHP = new Tuple<RegionType, int>[4]
        {
            Tuple.Create(RegionType.VP, 1),//6),
            Tuple.Create(RegionType.TB, 1),//4),
            Tuple.Create(RegionType.HP, 1),//4),
            Tuple.Create(RegionType.UES, 1),//10)
        };

        public static Tuple<RegionType, int>[] Sections()
        {
            return SectionsUsingTBAndHP;
        }

        public static IEnumerable<double> InterpolatedValuesAtSample(
            this ITimeFrame snapshot,
            TimeSample sample)
        {
            return Sections()
                .Select(p => InterpolatedValuesAtTimeForRegion(snapshot, sample, p.Item1, p.Item2)).SelectMany(m => m);
        }

        public static IEnumerable<double> InterpolateTo(this double[] source, int targetSize)
        {
            if (targetSize == source.Length)
                return source;
            if (source.Length == 1)
                return Enumerable.Repeat(source[0], targetSize);
            LinearSpline spline = LinearSpline.InterpolateSorted(XValues(source.Length).ToArray(), source);
            return XValues(targetSize).Select(x => spline.Interpolate(x));
        }

        private static IEnumerable<double> XValues(int count)
        {
            double sourceStep = 1.0 / (count - 1.0);
            return Enumerable.Range(0, count).Select(i => i * sourceStep);
        }

        private static IEnumerable<double> InterpolatedValuesAtTimeForRegion(
            ITimeFrame tFrame,
            TimeSample sample,
            RegionType regionType,
            int targetSize)
        {
            IRegion region = tFrame.GetRegion(regionType);
            return sample.ValuesForSensors(region.SensorRange).ToArray();//
            // return sample.ValuesForSensors(region.SensorRange).ToArray().InterpolateTo(targetSize);
        }
    }
}