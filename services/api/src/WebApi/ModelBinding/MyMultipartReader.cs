using ApiBuilder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using WebApi.Entities;

namespace WebApi.ModelBinding;

public class MyMultiPartReader : MultipartReader
{
    private readonly HttpContext _context;
    private readonly List<string> _validationErrors;
    private readonly FormOptions _formOptions;

    public MyMultiPartReader(HttpContext context, List<string> validationErrors)
        : base(context.GetBoundary(), context.Request.Body)
    {
        if (!context.IsMultipart())
        {
            throw new ModelBindingException("Content-Type must be multipart/form-data");
        }

        IOptions<FormOptions>? formOptions = context.RequestServices.GetService<IOptions<FormOptions>>();

        if (formOptions is null)
        {
            throw new InvalidOperationException("Missing form options");
        }

        _context = context;
        _validationErrors = validationErrors;
        _formOptions = formOptions.Value;
    }

    public new async Task<NextSection?> ReadNextSectionAsync(CancellationToken cancellationToken = default)
    {
        MultipartSection? section = await base.ReadNextSectionAsync(cancellationToken);
        return section is null ? null : new NextSection(section, _validationErrors);
    }

    public async Task<NextSection?> ReadNextSectionBufferedAsync(CancellationToken cancellationToken = default)
    {
        MultipartSection? section = await base.ReadNextSectionAsync(cancellationToken);

        if (section is null)
        {
            return null;
        }

        FileBufferingReadStream fileStream = new(section.Body, _formOptions.MemoryBufferThreshold,
            _formOptions.MultipartBodyLengthLimit, TempDirectory.Temp);
        section.Body = fileStream;
        _context.Response.RegisterForDisposeAsync(fileStream);

        return new NextSection(section, _validationErrors);
    }

    public async Task<IReadOnlyList<MyFormFile>?> ReadFilesBufferedAsync(CancellationToken cancellationToken = default)
    {
        List<MyFormFile> files = new();

        while (await ReadNextSectionBufferedAsync(cancellationToken) is { } nextSection)
        {
            MyFileMultipartSection? fileSection = nextSection.AsFileSection();

            if (fileSection is null)
            {
                return null;
            }

            await fileSection.Body.DrainAsync(cancellationToken);
            fileSection.Body.Position = 0;

            MyFormFile file = new(fileSection.Body, fileSection.Body.Length, fileSection.FileName);

            files.Add(file);
        }

        if (files.Count == 0)
        {
            _validationErrors.Add("Request does not contain any files");
            return null;
        }

        return files;
    }
}
