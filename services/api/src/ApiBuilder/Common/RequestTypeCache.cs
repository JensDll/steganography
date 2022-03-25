using System.Reflection;

namespace ApiBuilder;

internal static class RequestTypeCache<TRequest>
{
    static RequestTypeCache()
    {
        Type tRequest = typeof(TRequest);
        BindAsync = tRequest.GetMethod("BindAsync");
        Dispose = tRequest.GetMethod("Dispose");
    }

    internal static MethodInfo? BindAsync { get; }
    internal static MethodInfo? Dispose { get; }
}
