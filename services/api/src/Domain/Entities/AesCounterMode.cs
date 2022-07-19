using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Domain.Entities;

public class AesCounterMode : IDisposable
{
    private readonly Aes _aes;
    // The first 32 bits represent the counter and the last 96 bits are the IV.
    private readonly byte[] _counterAndIv = new byte[16];
    private readonly ICryptoTransform _encryptor;
    private readonly byte[] _keyStream = new byte[16];
    private int _keyStreamIdx;

    public AesCounterMode() :
        this(RandomNumberGenerator.GetBytes(32), RandomNumberGenerator.GetBytes(12))
    {
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
        _encryptor = _aes.CreateEncryptor();
        IV.CopyTo(_counterAndIv, 4);
        _encryptor.TransformBlock(_counterAndIv, 0, 16, _keyStream, 0);
    }

    public byte[] Key
    {
        get => _aes.Key;
        private init => _aes.Key = value;
    }

    public byte[] IV { get; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _aes.Dispose();
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GenerateNewKeyStream()
    {
        unsafe
        {
            fixed (byte* block = _counterAndIv)
            {
                uint* counter = (uint*) block;
                ++*counter;
            }
        }

        _keyStreamIdx = 0;
        _encryptor.TransformBlock(_counterAndIv, 0, 16, _keyStream, 0);
    }
}
