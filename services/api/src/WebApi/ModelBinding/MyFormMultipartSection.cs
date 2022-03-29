using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace WebApi.ModelBinding;

public class MyFormMultipartSection : MyMultipartSection
{
    private readonly MultipartSection _section;

    public MyFormMultipartSection(MultipartSection section, ContentDispositionHeaderValue contentDisposition,
        List<string> validationErrors) : base(section, contentDisposition, validationErrors)
    {
        _section = section;
    }

    public async Task<string> GetValueAsync()
    {
        return await _section.ReadAsStringAsync();
    }
}
