using Microsoft.AspNetCore.Routing;

namespace MiniApi;

public static class RouteValueDictionaryExtensions
{
    public static int GetInt(this RouteValueDictionary routeValues, string key)
    {
        if (!routeValues.TryGetValue(key, out object? objValue))
        {
            return default;
        }

        if (objValue is not string strValue)
        {
            return default;
        }

        return int.TryParse(strValue, out int number) ? number : default;
    }
}
