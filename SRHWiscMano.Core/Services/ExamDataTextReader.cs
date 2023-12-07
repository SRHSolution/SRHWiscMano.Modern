using System.IO;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.Services
{
    public class ExamDataTextReader : IImportService<IExamination> 
    {
        public IExamination? ReadFromFile(string filePath)
        {
            try
            {
                var parseSamples = LoadLines(File.ReadLines(filePath).Skip(1)).ToList();

                NoteXmlReader notesLoader = new NoteXmlReader();
                var notes = notesLoader.LoadRelative(filePath) ?? Array.Empty<FrameNote>();
                
                return new Examination(parseSamples, notes.ToList());
            }
            catch
            {
                return null;
            }
        }

        internal static IEnumerable<TimeSample> LoadLines(IEnumerable<string> lines)
        {
            return lines.Select(ParseLine);
        }

        private static TimeSample ParseLine(string line)
        {
            string[] source = line.Split('\t');
            long milliseconds = source.Length > 1
                ? TryParseTime(source[0])
                : throw new InvalidOperationException("Invalid file contents");
            List<double> values = new List<double>();
            List<double> doubleList = values;
            foreach (string s in source.Skip(1))
                if (string.IsNullOrEmpty(s))
                {
                    // Sample Data가 잘못되었음을 알리기 위함 
                    doubleList.Add(double.MaxValue);
                    //TODO : Warning 메시지 저장
                }
                else
                    doubleList.Add(s.AsDoubleOrDefault());
            return new TimeSample(InstantUtils.InstantFromMilliseconds(milliseconds), values);
        }

        private static long TryParseTime(string timeField)
        {
            long? nullable = timeField.AsLong();
            return !nullable.HasValue ? (long)(double.Parse(timeField) * 1000.0) : nullable.Value;
        }
    }
}
