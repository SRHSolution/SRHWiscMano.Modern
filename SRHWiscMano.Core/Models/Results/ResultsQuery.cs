using SRHWiscMano.Core.Helpers;
using Range = SRHWiscMano.Core.Helpers.Range;

namespace SRHWiscMano.Core.Models.Results
{
    public enum Sex
    {
        Unspecified,
        Male,
        Female,
        Other
    }

    public class ResultsQuery
    {
        public ResultsQuery(Range<int> ageRange, string volume)
        {
            AgeRange = ageRange;
            // Sex = sex;
            Volume = volume;
        }

        public Range<int> AgeRange { get; }

        public Sex Sex { get; }

        public string Volume { get; }

        public static ResultsQuery Load(
            int? ageRangeLow,
            int? ageRangeHigh,
            string sex,
            string volume)
        {
            return new ResultsQuery(
                !ageRangeLow.HasValue || !ageRangeHigh.HasValue
                    ? null
                    : Range.Create(ageRangeLow.Value, ageRangeHigh.Value),  volume);
        }
    }
}