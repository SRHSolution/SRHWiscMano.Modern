using SRHWiscMano.Core.Models.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRHWiscMano.Core.Helpers
{
    public static class ResultExtensions
    {
        public static string PrintSwallowResult(this SwallowResults<double> result)
        {
            var sb = new StringBuilder();
            sb.AppendLine(result.VP.PrintResultParameters("VP"));
            sb.AppendLine(result.PreUES.PrintResultParameters("PreUES"));
            sb.AppendLine(result.PostUES.PrintResultParameters("PostUES"));
            sb.AppendLine(result.TB.PrintResultParameters("TB"));
            sb.AppendLine(result.HP.PrintResultParameters("HP"));
            sb.AppendLine(result.UES.PrintResultParameters("UES"));

            sb.AppendLine($"TotalSwallowDuration : {result.TotalSwallowDuration}");
            sb.AppendLine($"MaxPressure : {result.MaxPressures.ToArray().ToStringJoin(",","F2")}");
            sb.AppendLine($"PressureAtVPMax : {result.PressureAtVPMax.ToArray().ToStringJoin(",", "F2")}");
            sb.AppendLine($"PressureAtTBMax : {result.PressureAtTBMax.ToArray().ToStringJoin(",", "F2")}");
            sb.AppendLine($"PressureGradient : {result.PressureGradient.ToArray().ToStringJoin(",", "F2")}");

            return sb.ToString();
        }

        public static string PrintResultParameters(this ResultParameters<double> result, string name)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"ResultParameters : {name}");
            sb.AppendLine($" -MaxPressure : {result.MaximumPressure}");
            sb.AppendLine($" -Duration : {result.Duration}");
            sb.AppendLine($" -TotVolume : {result.TotalVolumePressure}");
            sb.AppendLine($" -MaxVolume : {result.TotalPressureFromMaxSensor}");
            return sb.ToString();
        }

        public static string PrintSwallowResult(this SwallowResults<MeanAndDeviation> result)
        {
            var sb = new StringBuilder();
            sb.AppendLine(result.VP.PrintResultParameters("VP"));
            sb.AppendLine(result.PreUES.PrintResultParameters("PreUES"));
            sb.AppendLine(result.PostUES.PrintResultParameters("PostUES"));
            sb.AppendLine(result.TB.PrintResultParameters("TB"));
            sb.AppendLine(result.HP.PrintResultParameters("HP"));
            sb.AppendLine(result.UES.PrintResultParameters("UES"));

            sb.AppendLine($"TotalSwallowDuration : {result.TotalSwallowDuration}");
            sb.AppendLine($"MaxPressure : {result.MaxPressures.ToList().ToStringJoin()}");
            sb.AppendLine($"PressureAtVPMax : {result.PressureAtVPMax.ToList().ToStringJoin()}");
            sb.AppendLine($"PressureAtTBMax : {result.PressureAtTBMax.ToList().ToStringJoin()}");
            sb.AppendLine($"PressureGradient : {result.PressureGradient.ToList().ToStringJoin()}");

            return sb.ToString();
        }

        public static string PrintResultParameters(this ResultParameters<MeanAndDeviation> result, string name)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"ResultParameters : {name}");
            sb.AppendLine($" -MaxPressure : {result.MaximumPressure.PrintMeanAndDeviation()}");
            sb.AppendLine($" -Duration : {result.Duration.PrintMeanAndDeviation()}");
            sb.AppendLine($" -TotVolume : {result.TotalVolumePressure.PrintMeanAndDeviation()}");
            sb.AppendLine($" -MaxVolume : {result.TotalPressureFromMaxSensor.PrintMeanAndDeviation()}");
            return sb.ToString();
        }

        public static string PrintMeanAndDeviation(this MeanAndDeviation data)
        {
            return $"{data.Mean:F2}, {data.StandardDeviation:F2}, {data.Count}";
        }
    }
}
