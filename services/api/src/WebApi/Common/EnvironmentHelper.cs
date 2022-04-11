namespace WebApi.Common;

public static class EnvironmentHelper
{
    public static bool IsDocker()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_DOCKER") == "true";
    }
}
