using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NLog;
using SkiaSharp;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.Models.Results;

namespace SRHWiscMano.Core.Services
{
    public class ExamResultsOutlierExporter : IExportService<ExamResults<OutlierResult>>
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public void WriteToFile(ExamResults<OutlierResult> resultData, string filePath)
        {
            if (!Path.GetExtension(filePath).Equals(".csv", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new NotImplementedException("only CSV file export is available");
                return;
            }

            try
            {
                DoExportToCSV(resultData, filePath);
                // DoExportToCSVByRow(resultData, filePath);
            }
            catch (IOException ex)
            {
                _logger.Error(ex, $"IOException occurred while writing to the file: {filePath}");
                // Handle the exception (e.g., retry, notify user, etc.)
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.Error(ex, $"UnauthorizedAccessException occurred while accessing the file: {filePath}");
                // Handle the exception (e.g., check file permissions, notify user, etc.)
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"An unexpected error occurred while exporting exam results.");
                // Handle other potential exceptions
            }
        }

        private void DoExportToCSV(ExamResults<OutlierResult> resultData, string filePath)
        {
            StringBuilder sb = new StringBuilder();

            var tfIndivs = resultData.Individuals;

            sb.AppendLine("ID, Text, Time(s), TimeDuration(s), " +
                          "VPTimeDur(s), VPMaxPre(mmHg), VPTotPre(mmHg·s), " +
                          "TBTimeDur(s), TBMaxPre(mmHg), TBTotPre(mmHg·s), " +
                          "HPTimeDur(s), HPMaxPre(mmHg), HPTotPre(mmHg·s), " +
                          "PreUESTimeDur(s), PreUESMaxPre(mmHg), PreUESTotPre(mmHg·s), " +
                          "PostUESTimeDur(s), PostUESMaxPre(mmHg), PostUESTotPre(mmHg·s), " +
                          "UESTimeDur(s), UESMaxPre(mmHg), UESTotPre(mmHg·s)");

            foreach (var tFrameRes in tfIndivs)
            {
                var tFrame = tFrameRes.TimeFrame;
                sb.Append(
                    $"{tFrame.Id}, {tFrame.Text}, {(double) tFrame.Time.ToMillisecondsFromEpoch() / 1000:F2}, {tFrame.TimeDuration},");
                var tfRes = tFrameRes.Results;
                var region = tFrameRes.TimeFrame.GetRegion(RegionType.VP);
                sb.Append(
                    $"{tfRes.VP.Duration.Value}, {tfRes.VP.MaximumPressure.Value}, {tfRes.VP.TotalVolumePressure.Value},");
                region = tFrameRes.TimeFrame.GetRegion(RegionType.TB);
                sb.Append(
                    $"{tfRes.TB.Duration.Value}, {tfRes.TB.MaximumPressure.Value}, {tfRes.TB.TotalVolumePressure.Value},");
                region = tFrameRes.TimeFrame.GetRegion(RegionType.HP);
                sb.Append(
                    $"{tfRes.HP.Duration.Value}, {tfRes.HP.MaximumPressure.Value}, {tfRes.HP.TotalVolumePressure.Value},");
                region = tFrameRes.TimeFrame.GetRegion(RegionType.PreUES);
                sb.Append(
                    $"{tfRes.PreUES.Duration.Value}, {tfRes.PreUES.MaximumPressure.Value}, {tfRes.PreUES.TotalVolumePressure.Value},");
                region = tFrameRes.TimeFrame.GetRegion(RegionType.PostUES);
                sb.Append(
                    $"{tfRes.PostUES.Duration.Value}, {tfRes.PostUES.MaximumPressure.Value}, {tfRes.PostUES.TotalVolumePressure.Value},");
                region = tFrameRes.TimeFrame.GetRegion(RegionType.UES);
                sb.Append(
                    $"{tfRes.UES.Duration.Value}, {tfRes.UES.MaximumPressure.Value}, {tfRes.UES.TotalVolumePressure.Value}");
                sb.AppendLine();
            }

            File.WriteAllText(filePath, sb.ToString());
        }

        private void DoExportToCSVByRow(ExamResults<OutlierResult> resultData, string filePath)
        {
            StringBuilder sb = new StringBuilder();

            var tfIndivs = resultData.Individuals;

            // Collect all headers and their corresponding values
            var headers = new List<string>
            {
                "ID", "Text", "Time(s)", "TimeDuration(s)",
                "VPTimeDur(s)", "VPMaxPre(mmHg)", "VPTotPre(mmHg·s)",
                "TBTimeDur(s)", "TBMaxPre(mmHg)", "TBTotPre(mmHg·s)",
                "HPTimeDur(s)", "HPMaxPre(mmHg)", "HPTotPre(mmHg·s)",
                "PreUESTimeDur(s)", "PreUESMaxPre(mmHg)", "PreUESTotPre(mmHg·s)",
                "PostUESTimeDur(s)", "PostUESMaxPre(mmHg)", "PostUESTotPre(mmHg·s)",
                "UESTimeDur(s)", "UESMaxPre(mmHg)", "UESTotPre(mmHg·s)"
            };

            var dataRows = new List<List<string>>();

            foreach (var tFrameRes in tfIndivs)
            {
                var row = new List<string>();
                var tFrame = tFrameRes.TimeFrame;
                row.Add(tFrame.Id.ToString());
                row.Add(tFrame.Text);
                row.Add(((double) tFrame.Time.ToMillisecondsFromEpoch() / 1000).ToString("F2"));
                row.Add(tFrame.TimeDuration.ToString());

                var tfRes = tFrameRes.Results;
                var region = tFrameRes.TimeFrame.GetRegion(RegionType.VP);
                row.Add(tfRes.VP.Duration.Value.ToString());
                row.Add(tfRes.VP.MaximumPressure.Value.ToString());
                row.Add(tfRes.VP.TotalVolumePressure.Value.ToString());

                region = tFrameRes.TimeFrame.GetRegion(RegionType.TB);
                row.Add(tfRes.TB.Duration.Value.ToString());
                row.Add(tfRes.TB.MaximumPressure.Value.ToString());
                row.Add(tfRes.TB.TotalVolumePressure.Value.ToString());

                region = tFrameRes.TimeFrame.GetRegion(RegionType.HP);
                row.Add(tfRes.HP.Duration.Value.ToString());
                row.Add(tfRes.HP.MaximumPressure.Value.ToString());
                row.Add(tfRes.HP.TotalVolumePressure.Value.ToString());

                region = tFrameRes.TimeFrame.GetRegion(RegionType.PreUES);
                row.Add(tfRes.PreUES.Duration.Value.ToString());
                row.Add(tfRes.PreUES.MaximumPressure.Value.ToString());
                row.Add(tfRes.PreUES.TotalVolumePressure.Value.ToString());

                region = tFrameRes.TimeFrame.GetRegion(RegionType.PostUES);
                row.Add(tfRes.PostUES.Duration.Value.ToString());
                row.Add(tfRes.PostUES.MaximumPressure.Value.ToString());
                row.Add(tfRes.PostUES.TotalVolumePressure.Value.ToString());

                region = tFrameRes.TimeFrame.GetRegion(RegionType.UES);
                row.Add(tfRes.UES.Duration.Value.ToString());
                row.Add(tfRes.UES.MaximumPressure.Value.ToString());
                row.Add(tfRes.UES.TotalVolumePressure.Value.ToString());

                dataRows.Add(row);
            }

            // Write headers as rows
            for (int i = 0; i < headers.Count; i++)
            {
                sb.Append(headers[i]);
                foreach (var row in dataRows)
                {
                    sb.Append(",").Append(row[i]);
                }

                sb.AppendLine();
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
    }
}