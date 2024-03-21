namespace SRHWiscMano.Core.Services.Detection
{
    [Serializable]
    public class RegionFinderException : Exception
    {
        public RegionFinderException()
        {
        }

        public RegionFinderException(string message)
            : base(message)
        {
        }

        public RegionFinderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}