namespace SRHWiscMano.Core.Services.Report
{
    [Serializable]
    public class ResultsCalculationException : Exception
    {
        public ResultsCalculationException()
        {
        }

        public ResultsCalculationException(string message)
            : base(message)
        {
        }

        public ResultsCalculationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}