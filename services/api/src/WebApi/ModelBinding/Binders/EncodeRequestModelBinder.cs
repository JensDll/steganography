using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using SixLabors.ImageSharp;
using WebApi.Contracts.Request;
using WebApi.Helpers;

namespace WebApi.ModelBinding.Binders;

public class EncodeRequestModelBinder : IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        HttpRequest request = bindingContext.HttpContext.Request;

        if (!MultipartRequestHelper.IsMultipartContentType(request.ContentType))
        {
            bindingContext.ModelState.AddModelError("ContentType",
                "Content type is not multipart/form-data");
            return;
        }

        string boundary = MultipartRequestHelper.GetBoundary(
            MediaTypeHeaderValue.Parse(request.ContentType));
        MultipartReader multipartReader = new(boundary, request.Body);
        MultipartSection? section = await multipartReader.ReadNextSectionAsync();

        if (section != null)
        {
            bool hasContentDispositionHeader =
                ContentDispositionHeaderValue.TryParse(
                    section.ContentDisposition,
                    out ContentDispositionHeaderValue? contentDisposition);

            if (hasContentDispositionHeader && contentDisposition != null)
            {
                section.Body.Position = 0;

                if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition) &&
                    contentDisposition.Name == "coverImage")
                {
                    Image? coverImage = await Image.LoadAsync(section.Body);
                    EncodeRequest encodeRequest = new(coverImage, multipartReader);

                    bindingContext.Result = ModelBindingResult.Success(encodeRequest);
                }
                else
                {
                    bindingContext.ModelState.AddModelError("CoverImage", "Cover image is required");
                }
            }
        }
    }
}
