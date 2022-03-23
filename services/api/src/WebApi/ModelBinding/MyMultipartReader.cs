using ApiBuilder;
using Microsoft.AspNetCore.WebUtilities;

namespace WebApi.ModelBinding;

public class MyMultiPartReader
{
    private readonly MultipartReader _multipartReader;
    private readonly List<string> _validationErrors;

    public MyMultiPartReader(HttpContext context, List<string> validationErrors)
    {
        _validationErrors = validationErrors;

        if (!context.IsMultipartContentType())
        {
            throw new ModelBindingException("Content-Type must be multipart/form-data");
        }

        string boundary = context.GetBoundary();

        _multipartReader = new MultipartReader(boundary, context.Request.Body);
    }

    public async Task<NextPart?> ReadNextPartAsync(CancellationToken cancellationToken = default)
    {
        MultipartSection? section = await _multipartReader.ReadNextSectionAsync(cancellationToken);
        return section == null ? null : new NextPart(section, _validationErrors);
    }
}
