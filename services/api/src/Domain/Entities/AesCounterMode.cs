using System.Security.Cryptography;

namespace Domain.Entities;

public class AesCounterMode : IDisposable
{
    private readonly Aes _aes;
    private readonly ICryptoTransform _encryptor;
    private readonly byte[] _iVAndCounter = new byte[16];
    private readonly byte[] _keyStream = new byte[16];
    private int _keyStreamIdx;

    public byte[] Key
    {
        get => _aes.Key;
        private init => _aes.Key = value;
    }

    public byte[] IV { get; }

    public AesCounterMode()
    {
        _aes = Aes.Create();
        Key = RandomNumberGenerator.GetBytes(32);
        IV = RandomNumberGenerator.GetBytes(12);
        _aes.Mode = CipherMode.ECB;
        _aes.Padding = PaddingMode.None;
        _encryptor = _aes.CreateDecryptor();
        IV.CopyTo(_iVAndCounter, 4);
        _encryptor.TransformBlock(_iVAndCounter, 0, 16, _keyStream, 0);
    }

    public AesCounterMode(byte[] key, byte[] iV)
    {
        if (key.Length != 32)
        {
            throw new CryptographicException("Key must be 32 bytes long");
        }

        if (iV.Length != 12)
        {
            throw new CryptographicException("Initialization value must be 12 bytes long");
        }

        _aes = Aes.Create();
        Key = key;
        IV = iV;
        _aes.Mode = CipherMode.ECB;
        _aes.Padding = PaddingMode.None;
        _encryptor = _aes.CreateDecryptor();
        IV.CopyTo(_iVAndCounter, 4);
        _encryptor.TransformBlock(_iVAndCounter, 0, 16, _keyStream, 0);
    }

    public void Transform(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        for (int i = 0; i < source.Length; ++i)
        {
            destination[i] = (byte) (source[i] ^ _keyStream[_keyStreamIdx++]);
            if (_keyStreamIdx == _keyStream.Length)
            {
                GenerateNewKeyStream();
            }
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _aes.Dispose();
    }

    private unsafe void GenerateNewKeyStream()
    {
        fixed (byte* bytePointer = _iVAndCounter)
        {
            uint* counter = (uint*) bytePointer;
            ++*counter;
        }

        _keyStreamIdx = 0;
        _encryptor.TransformBlock(_iVAndCounter, 0, 16, _keyStream, 0);
    }
}
