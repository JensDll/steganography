using System.Net;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WebApi.ModelBinding;

public class NextPart
{
    private static readonly byte[] _emptyLength = new byte[4];
    private readonly HttpContext _context;
    private readonly MultipartSection _section;
    private readonly List<string> _validationErrors;

    public NextPart(MultipartSection section, HttpContext context, List<string> validationErrors)
    {
        _context = context;
        _validationErrors = validationErrors;
        _section = section;
        _section.Body.Position = 0;
    }

    public async Task<Image<Rgb24>?> ReadCoverImageAsync(string sectionName)
    {
        if (!_section.HasFileContentDisposition(sectionName, out _))
        {
            _validationErrors.Add($"Multipart section name does not match '{sectionName}'");
            return null;
        }

        Image<Rgb24>? coverImage;

        try
        {
            coverImage = await Image.LoadAsync<Rgb24>(_section.Body);
        }
        catch (UnknownImageFormatException)
        {
            _validationErrors.Add("Unsupported image format");
            return null;
        }

        long? messageLength = _context.Request.ContentLength - _section.Body.Length;

        if (messageLength >= _section.Body.Length)
        {
            coverImage.Dispose();
            _validationErrors.Add("Message is too large for the cover image");
            return null;
        }

        return coverImage;
    }

    public async Task<string?> ReadTextAsync(string sectionName)
    {
        if (!_section.HasFormDataContentDisposition(sectionName, out _))
        {
            _validationErrors.Add($"Multipart section name does not match '{sectionName}'");
            return null;
        }

        using StreamReader reader = new(_section.Body, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    public async Task CopyFileToAsync(Stream stream, string sectionName)
    {
        if (!_section.HasFileContentDisposition(sectionName, out ContentDispositionHeaderValue? contentDisposition))
        {
            _validationErrors.Add($"Multipart section name does not match '{sectionName}'");
            return;
        }

        string fileName = WebUtility.HtmlEncode(contentDisposition!.FileName.Value);
        byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);

        // Write the filename length
        await stream.WriteAsync(BitConverter.GetBytes(fileNameBytes.Length));
        // Write the filename
        await stream.WriteAsync(fileNameBytes);
        // Write the message length (zero first until the real length is available)
        await stream.WriteAsync(_emptyLength);
        // Write the message
        await _section.Body.CopyToAsync(stream);
        // Write the now available message length
        stream.Seek(-_section.Body.Length - 4, SeekOrigin.Current);
        await stream.WriteAsync(BitConverter.GetBytes((int) _section.Body.Length));
        stream.Seek(0, SeekOrigin.End);
    }

    public async Task CopyFormDataToAsync(Stream stream, string sectionName)
    {
        if (!_section.HasFormDataContentDisposition(sectionName, out _))
        {
            _validationErrors.Add($"Multipart section name does not match '{sectionName}'");
            return;
        }

        await _section.Body.CopyToAsync(stream);
    }
}
