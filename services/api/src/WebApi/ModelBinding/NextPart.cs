using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WebApi.ModelBinding;

public class NextPart
{
    private readonly MultipartSection _section;
    private readonly List<string> _validationErrors;

    public NextPart(MultipartSection section, List<string> validationErrors)
    {
        Body = section.Body;
        Body.Position = 0;

        _validationErrors = validationErrors;
        _section = section;
    }

    public Stream Body { get; }

    public async Task<Image<Rgb24>?> ReadCoverImageAsync(string sectionName,
        CancellationToken cancellationToken = default)
    {
        if (!IsFileContentDisposition(sectionName, out _))
        {
            return null;
        }

        Image<Rgb24>? coverImage;

        try
        {
            coverImage = await Image.LoadAsync<Rgb24>(Body, cancellationToken);
        }
        catch (UnknownImageFormatException)
        {
            _validationErrors.Add("Unsupported image format");
            return null;
        }

        return coverImage;
    }

    public async Task<string?> ReadTextAsync(string sectionName)
    {
        if (!IsFormDataContentDisposition(sectionName, out _))
        {
            return null;
        }

        using StreamReader reader = new(Body, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }


    public bool IsFileContentDisposition(string sectionName, out ContentDispositionHeaderValue? contentDisposition)
    {
        if (!IsFileContentDisposition(out contentDisposition))
        {
            return false;
        }

        if (contentDisposition!.Name.Equals(sectionName))
        {
            return true;
        }

        _validationErrors.Add($"Multipart section '{contentDisposition.Name}' does not match '{sectionName}'");
        return false;
    }

    public bool IsFileContentDisposition(out ContentDispositionHeaderValue? contentDisposition)
    {
        if (_section.IsFileContentDisposition(out contentDisposition))
        {
            return true;
        }

        _validationErrors.Add($"Multipart section '{contentDisposition?.Name}' is not a file");
        return false;
    }

    public bool IsFormDataContentDisposition(string sectionName, out ContentDispositionHeaderValue? contentDisposition)
    {
        if (!IsFormDataContentDisposition(out contentDisposition))
        {
            return false;
        }

        if (contentDisposition!.Name.Equals(sectionName))
        {
            return true;
        }

        _validationErrors.Add($"Multipart section '{contentDisposition.Name}' does not match '{sectionName}'");
        return false;
    }

    public bool IsFormDataContentDisposition(out ContentDispositionHeaderValue? contentDisposition)
    {
        if (_section.IsFormDataContentDisposition(out contentDisposition))
        {
            return true;
        }

        _validationErrors.Add($"Multipart section '{contentDisposition?.Name}' is not form data");
        return false;
    }
}
