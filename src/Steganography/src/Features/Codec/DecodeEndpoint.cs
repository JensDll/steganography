using System.IO.Compression;
using System.IO.Pipelines;
using Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using MinimalApiBuilder.Generator;
using MinimalApiBuilder.Multipart;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Steganography.Common;
using MultipartReader = MinimalApiBuilder.Multipart.MultipartReader;
#if NOT_RUNNING_IN_CONTAINER
using Microsoft.OpenApi.Models;
#endif

namespace Steganography.Features.Codec;

internal partial class DecodeEndpoint : MinimalApiBuilderEndpoint
{
    public static void Configure(RouteHandlerBuilder builder)
    {
        builder.Produces(StatusCodes.Status200OK, contentType: "text/plain",
            additionalContentTypes: "application/zip");
#if NOT_RUNNING_IN_CONTAINER
        builder.WithOpenApi(static operation => new OpenApiOperation(operation)
        {
            RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new()
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["coverImage"] = new()
                                {
                                    Type = "string",
                                    Format = "binary"
                                },
                                ["key"] = new()
                                {
                                    Type = "string"
                                }
                            },
                            Required = new HashSet<string>
                            {
                                "coverImage",
                                "key"
                            }
                        }
                    }
                }
            }
        });
#endif
    }

    public static async Task<Results<EmptyHttpResult, ValidationProblem>> Handle(
        DecodeRequest request,
        [FromServices] DecodeEndpoint endpoint,
        [FromServices] IKeyService keyService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        bool isValidKey = keyService.TryParse(request.Key, out MessageType messageType, out int seed,
            out int messageLength, out byte[] key, out byte[] initValue);

        if (!isValidKey || messageLength < 1 || messageLength > request.CoverImageCapacity)
        {
            return TypedResults.ValidationProblem(endpoint.ValidationErrors, title: "Decoding failed.");
        }

        using AesCounterMode aes = new(key, initValue);
        using ImageDecoder imageDecoder = new(request.CoverImage, seed, messageLength, aes);

        if (messageType == MessageType.Text)
        {
            httpContext.Response.ContentType = "text/plain";

            try
            {
                await imageDecoder.DecodeAsync(httpContext.Response.BodyWriter, cancellationToken);
            }
            catch (OperationCanceledException)
            { }

            return TypedResults.Empty;
        }

        ContentDispositionHeaderValue contentDisposition = new("attachment");
        contentDisposition.SetHttpFileName("result.zip");
        httpContext.Response.Headers.ContentDisposition = contentDisposition.ToString();
        httpContext.Response.ContentType = "application/zip";

        using ZipArchive archive = new(httpContext.Response.BodyWriter.AsStream(), ZipArchiveMode.Create);

        while (imageDecoder.DecodeNextFileInformation() is var (fileName, fileLength))
        {
            ZipArchiveEntry entry = archive.CreateEntry(fileName, CompressionLevel.Fastest);
            await using Stream entryStream = entry.Open();
            PipeWriter entryStreamWriter = PipeWriter.Create(entryStream);
            try
            {
                await imageDecoder.DecodeAsync(entryStreamWriter, fileLength, cancellationToken);
            }
            catch (OperationCanceledException)
            { }
        }

        return TypedResults.Empty;
    }
}

public class DecodeRequest
{
    public required Image<Rgb24> CoverImage { get; init; }

    public required int CoverImageCapacity { get; init; }

    public required string Key { get; init; }

    public static async ValueTask<DecodeRequest?> BindAsync(HttpContext context)
    {
        CancellationToken cancellationToken = context.RequestAborted;
        DecodeEndpoint endpoint = context.RequestServices.GetRequiredService<DecodeEndpoint>();
        MultipartReader multipartReader = new(context, endpoint);

        if (endpoint.HasValidationError)
        {
            return null;
        }

        NextSection? nextSection = await multipartReader.ReadNextSectionAsync(cancellationToken);
        FileMultipartSection? fileSection = nextSection?.AsFileSection("coverImage");

        if (fileSection is null)
        {
            return null;
        }

        Image<Rgb24>? coverImage = await fileSection.ReadCoverImageAsync(cancellationToken);

        if (coverImage is null)
        {
            endpoint.AddValidationError("cover-image", "The cover image is invalid.");
            return null;
        }

        context.Response.RegisterForDispose(coverImage);

        if (coverImage.Metadata.DecodedImageFormat is not PngFormat)
        {
            endpoint.AddValidationError("cover-image", "The cover image must be a PNG image.");
            return null;
        }

        nextSection = await multipartReader.ReadNextSectionAsync(cancellationToken);
        FormMultipartSection? formSection = nextSection?.AsFormSection("key");

        if (formSection is null)
        {
            return null;
        }

        string key = await formSection.GetValueAsync(cancellationToken);

        return new DecodeRequest
        {
            CoverImage = coverImage,
            CoverImageCapacity = coverImage.Width * coverImage.Height * 3,
            Key = key
        };
    }
}
