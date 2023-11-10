namespace SRHWiscMano.Core.Services
{
    public interface IImportService<T>
    {
        T? ReadFromFile(string filePath);
    }
}
