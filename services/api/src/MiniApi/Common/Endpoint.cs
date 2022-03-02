namespace MiniApi;

public abstract class Endpoint<TRequest> : EndpointBase
    where TRequest : Request, new()
{
    public abstract Task HandleAsync(TRequest request);
}

public abstract class Endpoint<TRequest, TResponse> : EndpointBase
    where TRequest : Request, new()
    where TResponse : notnull
{
    public abstract Task<TResponse> HandleAsync(TRequest request);
}
