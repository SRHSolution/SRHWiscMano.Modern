namespace SRHWiscMano.Core.Models.Results
{
    public class TimeFrameResult<T>
    {
        private readonly SwallowResults<T> results;
        private readonly ITimeFrame timeFrame;

        public TimeFrameResult(ITimeFrame timeFrame, SwallowResults<T> results)
        {
            this.timeFrame = timeFrame;
            this.results = results;
        }

        public ITimeFrame TimeFrame => timeFrame;

        public SwallowResults<T> Results => results;
    }
}