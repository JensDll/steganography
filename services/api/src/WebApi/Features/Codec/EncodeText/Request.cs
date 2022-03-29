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

    public PipeReader PipeReader { get; private set; } = null!;

    public CancellationTokenSource CancelSource { get; } = new();

    public async ValueTask BindAsync(HttpContext context, List<string> validationErrors,
        CancellationToken cancellationToken)
    {
        _validationErrors = validationErrors;
        _multiPartReader = new MyMultiPartReader(context, validationErrors);

        NextSection? nextSection = await _multiPartReader.ReadNextSectionAsync(cancellationToken);

        if (nextSection is null)
        {
            validationErrors.Add("Request does not contain a cover image");
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

        Pipe pipe = new();
        PipeReader = pipe.Reader;
        _pipeWriter = pipe.Writer;
    }

    public async Task<int?> FillPipeAsync(AesCounterMode aes)
    {
        NextSection? nextSection = await _multiPartReader.ReadNextSectionAsync(CancelSource.Token);

        if (nextSection is null)
        {
            _validationErrors.Add("Request does not contain a message");
            CancelSource.Cancel();
            return null;
        }

        MyFormMultipartSection? formSection = nextSection.AsFormSection("message");

        if (formSection is null)
        {
            CancelSource.Cancel();
            return null;
        }

        int messageLength = 0;

        while (true)
        {
            Memory<byte> buffer = _pipeWriter.GetMemory();

            int bytesRead = await formSection.Body.ReadAsync(buffer, CancelSource.Token);

            if (bytesRead == 0)
            {
                break;
            }

            messageLength += bytesRead;
            aes.Transform(buffer.Span[..bytesRead], buffer.Span);
            _pipeWriter.Advance(bytesRead);

            FlushResult result = await _pipeWriter.FlushAsync(CancelSource.Token);

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
        // ReSharper disable once ConstantConditionalAccessQualifier
        CoverImage?.Dispose();
        CancelSource.Dispose();
    }
}
