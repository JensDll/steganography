using System.IO.Pipelines;
using System.Text;
using ApiBuilder;
using Domain.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WebApi.ModelBinding;

namespace WebApi.Features.Codec.EncodeBinary;

public class Request : IBindRequest, IDisposable
{
    private List<string> _validationErrors = null!;
    private MyMultiPartReader _multiPartReader = null!;
    private PipeWriter _pipeWriter = null!;

    public Image<Rgb24> CoverImage { get; private set; } = null!;

    public PipeReader PipeReader { get; private set; } = null!;

    public int CoverImageCapacity { get; private set; }

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
        CoverImageCapacity = coverImage.Width * coverImage.Height * 3;

        Pipe pipe = new();
        PipeReader = pipe.Reader;
        _pipeWriter = pipe.Writer;
    }

    public async Task<int?> FillPipeAsync(AesCounterMode aes)
    {
        IReadOnlyList<MyFormFile>? files = await _multiPartReader.ReadFilesBufferedAsync();

        if (files is null)
        {
            PipeReader.CancelPendingRead();
            await _pipeWriter.CompleteAsync();
            return null;
        }

        int messageLength = 0;

        foreach (MyFormFile file in files)
        {
            int fileNameSize = Encoding.UTF8.GetByteCount(file.FileName);

            if (fileNameSize > 256)
            {
                throw new ModelBindingException("File name can not be longer than 256 bytes");
            }

            int sizeHint = 6 + fileNameSize;
            messageLength += sizeHint + file.Length;

            Memory<byte> buffer = _pipeWriter.GetMemory(sizeHint);
            // Write the file length (4-byte)
            BitConverter.TryWriteBytes(buffer.Span, file.Length);
            // Write the file name length (2-byte)
            BitConverter.TryWriteBytes(buffer.Span[4..], (short) fileNameSize);
            // Write the file name (max 256-byte)
            Encoding.UTF8.GetBytes(file.FileName, buffer.Span[6..]);
            aes.Transform(buffer.Span[..sizeHint], buffer.Span);
            _pipeWriter.Advance(sizeHint);
        }

        if (messageLength > CoverImageCapacity)
        {
            _validationErrors.Add("Message is too large for the cover image");
            PipeReader.CancelPendingRead();
            await _pipeWriter.CompleteAsync();
            return null;
        }

        foreach (MyFormFile file in files)
        {
            while (true)
            {
                Memory<byte> buffer = _pipeWriter.GetMemory();
                int bytesRead = await file.ReadAsync(buffer);

                if (bytesRead == 0)
                {
                    break;
                }

                aes.Transform(buffer.Span[..bytesRead], buffer.Span);
                _pipeWriter.Advance(bytesRead);

                FlushResult result = await _pipeWriter.FlushAsync();

                if (result.IsCompleted)
                {
                    break;
                }
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
    }
}
