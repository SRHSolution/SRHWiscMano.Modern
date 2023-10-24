namespace SRHWiscMano.Core.Helpers
{
    public static class VisualStudioProvider
    {
        public static DirectoryInfo TryGetSolutionDirectoryInfo(string currentPath = null)
        {
            var directory = new DirectoryInfo(
                currentPath ?? Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }
            return directory;
        }


        public static string GetPathInSolution(string addedPath)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var solutionDir = TryGetSolutionDirectoryInfo(baseDir).FullName;

            var testPath = Path.Combine(solutionDir, addedPath);

            return testPath;
        }

        public static string GetPathInBase(string addedPath)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var testPath = Path.Combine(baseDir, addedPath);

            return testPath;
        }
    }
}
