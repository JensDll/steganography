using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace WebApi.ModelBinding;

public abstract class MyMultipartSection
{
    protected MyMultipartSection(MultipartSection section, ContentDispositionHeaderValue contentDisposition,
        List<string> validationErrors)
    {
        Name = HeaderUtilities.RemoveQuotes(contentDisposition.Name).ToString();
        Body = section.Body;
        ValidationErrors = validationErrors;
    }

    public string Name { get; }

    public Stream Body { get; }

    protected List<string> ValidationErrors { get; }
}
