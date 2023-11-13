namespace api.extensions;

internal static class EnvironmentExtensions
{
    public static bool IsRunningInContainer(this IWebHostEnvironment environment)
    {
        return environment.IsProduction() &&
               Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") is "true";
    }
}
