using Microsoft.AspNetCore.Mvc;
using Contracts;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Headers;

namespace WebAPI.Controllers;

[ApiController]
public class CodecController : ControllerBase
{

    [HttpGet(ApiRoutes.CodecRoutes.Encode)]
    public IActionResult Encode()
    {
        
        if (!Request.HasFormContentType ||
            !MediaTypeHeaderValue.TryParse(Request.ContentType, out var mediaTypeHeaderValue))
        {
            return new UnsupportedMediaTypeResult();
        }

        return Ok("Convert.ToBase64String(bytes)");
    }
}

