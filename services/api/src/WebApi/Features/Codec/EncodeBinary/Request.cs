using System.IO.Pipelines;
using System.Text;
using ApiBuilder;
using Domain.Entities;
using Microsoft.Net.Http.Headers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WebApi.ModelBinding;

namespace WebApi.Features.Codec.EncodeBinary;

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

    /// <summary>
    /// Reads the multipart request body, encrypts it, and writes it to the pipe.
    /// The written data has the form: <c>{file length}{file name length}{file name}{file data}</c>.
    /// </summary>
    /// <param name="aes">The aes instance used for encryption.</param>
    /// <returns><c>True</c> if the whole message was written successfully, otherwise <c>false</c>.</returns>
    public async Task<bool> FillPipeAsync(AesCounterMode aes)
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
                if (!nextPart.IsFormData(out _))
                {
                    CancelSource.Cancel();
                    return false;
                }
            }
            else
            {
                if (!nextPart.IsFile(out ContentDispositionHeaderValue? fileContentDisposition))
                {
                    CancelSource.Cancel();
                    return false;
                }

                int size = Encoding.UTF8.GetByteCount(fileContentDisposition!.FileName) + sizeof(int);
                Memory<byte> buffer = _pipeWriter.GetMemory(size);
                // Write the file name length to the first 4 bytes
                BitConverter.TryWriteBytes(buffer.Span, fileContentDisposition.FileName.Length);
                // Write the file name after that
                Encoding.UTF8.GetBytes(fileContentDisposition.FileName, buffer[sizeof(int)..].Span);
                aes.Transform(buffer[..size].Span, buffer.Span);
                _pipeWriter.Advance(size);
            }

            while (true)
            {
                Memory<byte> buffer = _pipeWriter.GetMemory();
                int bytesRead = await nextPart.Body.ReadAsync(buffer, CancelSource.Token);

                if (bytesRead == 0)
                {
                    break;
                }

                if (isFileLength)
                {
                    ParsingUtils.CopyAsInt32(buffer[..bytesRead].Span, buffer.Span);
                    aes.Transform(buffer[..sizeof(int)].Span, buffer.Span);
                    _pipeWriter.Advance(sizeof(int));
                }
                else
                {
                    aes.Transform(buffer[..bytesRead].Span, buffer.Span);
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

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        // ReSharper disable once ConstantConditionalAccessQualifier
        CoverImage?.Dispose();
        CancelSource.Dispose();
    }
}
