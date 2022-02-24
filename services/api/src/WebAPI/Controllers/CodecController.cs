using System.Security.Cryptography;
using System.Text;
using Contracts;
using Domain.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using SixLabors.ImageSharp;
using WebApi.Helpers;

namespace WebApi.Controllers;

[ApiController]
public class CodecController : ControllerBase
{
    private readonly string[] _permittedExtensions = {".png"};

    [HttpPost(ApiRoutes.CodecRoutes.EncodeText)]
    public async Task<IActionResult> EncodeText()
    {
        if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType)) return BadRequest();

        string boundary = MultipartRequestHelper.GetBoundary(
            MediaTypeHeaderValue.Parse(Request.ContentType));
        MultipartReader multipartReader = new(boundary, Request.Body);
        MultipartSection? section = await multipartReader.ReadNextSectionAsync();

        while (section != null)
        {
            bool hasContentDispositionHeader =
                ContentDispositionHeaderValue.TryParse(
                    section.ContentDisposition,
                    out ContentDispositionHeaderValue? contentDisposition);

            if (hasContentDispositionHeader && contentDisposition != null)
            {
                section.Body.Position = 0;

                if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                {
                    using Image? image = await Image.LoadAsync(section.Body);

                    int width = image.Width;
                }
                else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                {
                    using StreamReader streamReader = new(section.Body, Encoding.UTF8);
                    string formData = await streamReader.ReadToEndAsync();
                }
                else
                {
                    return BadRequest();
                }
            }

            section = await multipartReader.ReadNextSectionAsync();
        }

        return Ok();
    }

    [HttpPost(ApiRoutes.CodecRoutes.EncodeBinary)]
    public IActionResult EncodeBinary()
    {
        RandomNumberGenerator rng = RandomNumberGenerator.Create();

        return Ok(rng.GenerateKey(256));
    }
}
