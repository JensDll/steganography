using System.Text.Json;

namespace WebApi.Extensions;

public static class HttpResponseExtensions
{
    public static async Task SendEventStreamAsync<T>(this HttpResponse response,
        string eventName,
        IAsyncEnumerable<T> eventStream,
        CancellationToken cancellationToken)
    {
        response.StatusCode = 200;
        response.ContentType = "text/event-stream";
        response.Headers.CacheControl = "no-cache";

        long id = 0;

        await foreach (T item in eventStream.WithCancellation(cancellationToken))
        {
            ++id;
            await response.WriteAsync(
                text: $"id:{id}\nevent: {eventName}\ndata: {JsonSerializer.Serialize(item)}\n\n",
                cancellationToken: cancellationToken);
        }
    }
}
