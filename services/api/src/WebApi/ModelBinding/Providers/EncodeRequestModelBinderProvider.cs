using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebApi.Contracts.Request;
using WebApi.ModelBinding.Binders;

namespace WebApi.ModelBinding.Providers;

public class EncodeRequestModelBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (context.Metadata.ModelType == typeof(EncodeRequest))
        {
            return new EncodeRequestModelBinder();
        }

        throw new ArgumentException($"Invalid model type: {context.Metadata.ModelType.FullName}");
    }
}
