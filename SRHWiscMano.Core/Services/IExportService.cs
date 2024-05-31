namespace SRHWiscMano.Core.Services
{
    public interface IExportService<T>
    {
        void WriteToFile(T resultData, string filePath);
    }
}
