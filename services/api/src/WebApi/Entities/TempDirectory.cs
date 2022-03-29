namespace WebApi.Entities;

public static class TempDirectory
{
    private static string? _tempDirectory;

    public static string Temp
    {
        get
        {
            if (_tempDirectory is not null)
            {
                return _tempDirectory;
            }

            string tempDirectory = Environment.GetEnvironmentVariable("ASPNETCORE_TEMP") ?? Path.GetTempPath();

            if (!Directory.Exists(tempDirectory))
            {
                throw new DirectoryNotFoundException(tempDirectory);
            }

            _tempDirectory = tempDirectory;

            return _tempDirectory;
        }
    }
}
