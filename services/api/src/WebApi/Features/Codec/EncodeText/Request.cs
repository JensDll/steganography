using System.IO.Pipelines;
using ApiBuilder;
using Domain.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WebApi.ModelBinding;

namespace WebApi.Features.Codec.EncodeText;

public class Request : IBindRequest
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

        NextPart? nextPart = await _multiPartReader.ReadNextPartAsync(cancellationToken);

        if (nextPart == null)
        {
            validationErrors.Add("Request is empty");
            return;
        }

        Image<Rgb24>? coverImage = await nextPart.ReadCoverImageAsync("coverImage", cancellationToken);

        if (coverImage == null)
        {
            return;
        }

        CoverImage = coverImage;

        Pipe pipe = new();
        PipeReader = pipe.Reader;
        _pipeWriter = pipe.Writer;
    }

    public async Task<bool> FillPipeAsync(AesCounterMode aes)
    {
        NextPart? nextPart = await _multiPartReader.ReadNextPartAsync(CancelSource.Token);

        if (nextPart == null)
        {
            CancelSource.Cancel();
            _validationErrors.Add("Request does not contain a message");
            return false;
        }

        if (!nextPart.IsFormDataContentDisposition("message", out _))
        {
            CancelSource.Cancel();
            return false;
        }

        while (true)
        {
            Memory<byte> buffer = _pipeWriter.GetMemory();

            int bytesRead = await nextPart.Body.ReadAsync(buffer, CancelSource.Token);

            if (bytesRead == 0)
            {
                break;
            }

            aes.Transform(buffer[..bytesRead].Span, buffer.Span);
            _pipeWriter.Advance(bytesRead);

            FlushResult result = await _pipeWriter.FlushAsync(CancelSource.Token);

            if (result.IsCompleted)
            {
                break;
            }
        }

        await _pipeWriter.CompleteAsync();
        return true;
    }
}
