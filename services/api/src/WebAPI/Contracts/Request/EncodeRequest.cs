using Microsoft.AspNetCore.WebUtilities;
using SixLabors.ImageSharp;

namespace WebApi.Contracts.Request;

public class EncodeRequest
{
    public EncodeRequest(Image coverImage, MultipartReader multipartReader)
    {
        CoverImage = coverImage;
        MultipartReader = multipartReader;
    }

    public Image CoverImage { get; }

    public MultipartReader MultipartReader { get; }
}
