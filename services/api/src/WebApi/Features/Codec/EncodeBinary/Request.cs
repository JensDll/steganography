using System.IO.Pipelines;
using System.Security.Cryptography;
using System.Text;
using ApiBuilder;
using Domain.Entities;
using Microsoft.Net.Http.Headers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WebApi.ModelBinding;

namespace WebApi.Features.Codec.EncodeBinary;

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

    public async Task<bool> FillPipeAsync(ICryptoTransform encryptor)
    {
        NextPart? nextPart = await _multiPartReader.ReadNextPartAsync(CancelSource.Token);

        if (nextPart == null)
        {
            CancelSource.Cancel();
            _validationErrors.Add("Request does not contain a message");
            return false;
        }

        int sectionCount = 0;
        while (nextPart != null)
        {
            bool isFileLength = sectionCount++ % 2 == 0;

            if (isFileLength)
            {
                if (!nextPart.IsFormDataContentDisposition(out _))
                {
                    CancelSource.Cancel();
                    return false;
                }
            }
            else
            {
                if (!nextPart.IsFileContentDisposition(out ContentDispositionHeaderValue? fileContentDisposition))
                {
                    CancelSource.Cancel();
                    return false;
                }

                byte[] fileName = Encoding.UTF8.GetBytes(fileContentDisposition!.FileName.Value);

                await _pipeWriter.WriteAsync(BitConverter.GetBytes(fileName.Length), CancelSource.Token);
                await _pipeWriter.WriteAsync(fileName, CancelSource.Token);
            }

            while (true)
            {
                Memory<byte> buffer = _pipeWriter.GetMemory(512);
                int bytesRead = await nextPart.Body.ReadAsync(buffer, CancelSource.Token);

                if (bytesRead == 0)
                {
                    break;
                }

                if (isFileLength)
                {
                    ParsingUtils.CopyAsInt32(buffer[..bytesRead].Span, buffer.Span);
                    _pipeWriter.Advance(4);
                }
                else
                {
                    _pipeWriter.Advance(bytesRead);
                }

                FlushResult result = await _pipeWriter.FlushAsync(CancelSource.Token);

                if (result.IsCompleted)
                {
                    break;
                }
            }

            nextPart = await _multiPartReader.ReadNextPartAsync(CancelSource.Token);
        }

        await _pipeWriter.CompleteAsync();
        return true;
    }
}
