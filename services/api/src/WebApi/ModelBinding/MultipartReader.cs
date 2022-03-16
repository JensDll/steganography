using ApiBuilder;
using Microsoft.AspNetCore.WebUtilities;

namespace WebApi.ModelBinding;

public class MultiPartReader
{
    private readonly HttpContext _context;
    private readonly MultipartReader _multipartReader;
    private readonly List<string> _validationErrors;

    public MultiPartReader(HttpContext context, List<string> validationErrors)
    {
        _context = context;
        _validationErrors = validationErrors;

        if (!_context.IsMultipartContentType())
        {
            throw new ModelBindingException("Content-Type must be multipart/form-data");
        }

        string boundary = _context.GetBoundary();

        _multipartReader = new MultipartReader(boundary, _context.Request.Body);
    }

    public bool HasError => _validationErrors.Any();

    public async Task<NextPart?> ReadNextPartAsync()
    {
        MultipartSection? section = await _multipartReader.ReadNextSectionAsync();
        return section == null ? null : new NextPart(section, _context, _validationErrors);
    }
}
