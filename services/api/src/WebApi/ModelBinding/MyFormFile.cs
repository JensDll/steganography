namespace WebApi.ModelBinding;

public class MyFormFile
{
    private readonly Stream _baseStream;

    public MyFormFile(Stream baseStream, long length, string fileName)
    {
        _baseStream = baseStream;
        Length = (int) length;
        FileName = fileName;
    }

    public int Length { get; }

    public string FileName { get; set; }

    public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return _baseStream.ReadAsync(buffer, cancellationToken);
    }
}
