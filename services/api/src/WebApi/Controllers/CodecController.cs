using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using WebApi.Attributes;
using WebApi.Contracts;
using WebApi.Contracts.Request;
using WebApi.Helpers;

namespace WebApi.Controllers;

[ApiController]
[DisableFormDataModelBinding]
public class CodecController : ControllerBase
{
    private readonly IKeyGenerator _keyGenerator;

    public CodecController(IKeyGenerator keyGenerator)
    {
        _keyGenerator = keyGenerator;
    }

    [HttpPost(ApiRoutes.CodecRoutes.EncodeText)]
    public async Task<IActionResult> EncodeText(EncodeRequest encodeRequest)
    {
        MultipartSection? section = await encodeRequest.MultipartReader.ReadNextSectionAsync();

        while (section != null)
        {
            bool hasContentDispositionHeader =
                ContentDispositionHeaderValue.TryParse(
                    section.ContentDisposition,
                    out ContentDispositionHeaderValue? contentDisposition);

            if (hasContentDispositionHeader && contentDisposition != null)
            {
                section.Body.Position = 0;

                if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                {
                    await using MemoryStream memoryStream = new();
                    await section.Body.CopyToAsync(memoryStream);
                    byte[] data = memoryStream.ToArray();
                }
                else
                {
                    return ValidationProblem();
                }
            }

            section = await encodeRequest.MultipartReader.ReadNextSectionAsync();
        }

        return Ok();
    }

    [HttpPost(ApiRoutes.CodecRoutes.EncodeBinary)]
    public IActionResult EncodeBinary(EncodeRequest encodeRequest)
    {
        string key = _keyGenerator.GenerateKey(256);
        return Ok(key);
    }
}
