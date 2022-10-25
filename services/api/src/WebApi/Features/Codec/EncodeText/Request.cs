using System.IO.Pipelines;
using ApiBuilder;
using Domain.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WebApi.ModelBinding;

namespace WebApi.Features.Codec.EncodeText;

public class Request : IBindRequest, IDisposable
{
    private MyMultiPartReader _multiPartReader = null!;
    private PipeWriter _pipeWriter = null!;
    private List<string> _validationErrors = null!;

    public Image<Rgb24> CoverImage { get; private set; } = null!;

    public int CoverImageCapacity { get; private set; }

    public PipeReader PipeReader { get; private set; } = null!;

    public async ValueTask BindAsync(HttpContext context, List<string> validationErrors,
        CancellationToken cancellationToken)
    {
        _validationErrors = validationErrors;
        _multiPartReader = new MyMultiPartReader(context, validationErrors);

        NextSection? nextSection = await _multiPartReader.ReadNextSectionAsync(cancellationToken);

        if (nextSection is null)
        {
            validationErrors.Add("The request does not contain a cover image");
            return;
        }

        MyFileMultipartSection? fileSection = nextSection.AsFileSection("coverImage");

        if (fileSection is null)
        {
            return;
        }

        Image<Rgb24>? coverImage = await fileSection.ReadCoverImageAsync(cancellationToken);

        if (coverImage is null)
        {
            return;
        }

        CoverImage = coverImage;
        CoverImageCapacity = coverImage.Width * coverImage.Height * 3;

        Pipe pipe = new();
        PipeReader = pipe.Reader;
        _pipeWriter = pipe.Writer;
    }

    public async Task<int?> FillPipeAsync(AesCounterMode aes, CancellationToken cancellationToken)
    {
        NextSection? nextSection = await _multiPartReader.ReadNextSectionAsync(cancellationToken);

        if (nextSection is null)
        {
            _validationErrors.Add("The request does not contain a message");
            PipeReader.CancelPendingRead();
            await _pipeWriter.CompleteAsync();
            return null;
        }

        MyFormMultipartSection? formSection = nextSection.AsFormSection("message");

        if (formSection is null)
        {
            PipeReader.CancelPendingRead();
            await _pipeWriter.CompleteAsync();
            return null;
        }

        int messageLength = 0;

        while (true)
        {
            Memory<byte> buffer = _pipeWriter.GetMemory();

            int bytesRead = await formSection.Body.ReadAsync(buffer, cancellationToken);

            if (bytesRead == 0)
            {
                break;
            }

            messageLength += bytesRead;

            if (messageLength > CoverImageCapacity)
            {
                _validationErrors.Add("The message is too large for the cover image");
                PipeReader.CancelPendingRead();
                _pipeWriter.CancelPendingFlush();
                await _pipeWriter.FlushAsync(cancellationToken);
                await _pipeWriter.CompleteAsync();
                return null;
            }

            aes.Transform(buffer.Span[..bytesRead], buffer.Span);
            _pipeWriter.Advance(bytesRead);

            FlushResult result = await _pipeWriter.FlushAsync(cancellationToken);

            if (result.IsCompleted)
            {
                break;
            }
        }

        await _pipeWriter.CompleteAsync();

        return messageLength;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        CoverImage?.Dispose();
    }
}
