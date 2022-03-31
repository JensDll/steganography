namespace WebApi.Entities;

public static class TempDirectory
{
    static TempDirectory()
    {
        string tempDirectory = Environment.GetEnvironmentVariable("ASPNETCORE_TEMP") ?? System.IO.Path.GetTempPath();

        if (!Directory.Exists(tempDirectory))
        {
            throw new DirectoryNotFoundException(tempDirectory);
        }

        Path = tempDirectory;
    }

    public static string Path { get; }
}
