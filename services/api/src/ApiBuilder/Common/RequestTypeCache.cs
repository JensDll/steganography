using System.Reflection;

namespace ApiBuilder;

internal static class RequestTypeCache<TRequest>
{
    static RequestTypeCache()
    {
        Type tRequest = typeof(TRequest);
        BindAsync = tRequest.GetMethod("BindAsync");
    }

    internal static MethodInfo? BindAsync { get; }
}
