using NodaTime;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.Models.Results;

namespace SRHWiscMano.Core.Services.Report
{
    internal static class OverallParamsCalculator
    {
        public static Duration CalcTotalDuration(ITimeFrame snapshot)
        {
            IRegion region = snapshot.GetRegion(RegionType.VP);
            return snapshot.GetRegion(RegionType.PostUES).FocalPoint.Time - region.TimeRange.Start;
        }

        public static double CalcTotalPharyngealPressure(
            ITimeFrame snapshot,
            SwallowResults<double> currentResults)
        {
            return PharyngealRegions(currentResults).Sum(r => r.TotalVolumePressure);
        }

        private static IEnumerable<ResultParameters<double>> PharyngealRegions(
            SwallowResults<double> currentResults)
        {
            yield return currentResults.VP;
            yield return currentResults.TB;
            yield return currentResults.HP;
        }
    }
}