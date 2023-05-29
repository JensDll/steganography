using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace steganography.domain;

public class AesCounterMode : IDisposable
{
    private readonly Aes _aes;
    // The first 4-byte represent the counter and the last 12-byte are the initialization value
    private readonly byte[] _counterAndInitializationValue = new byte[16];
    private readonly ICryptoTransform _encryptor;
    private readonly byte[] _keyStream = new byte[16];
    private int _keyStreamIdx;

    public AesCounterMode() :
        this(RandomNumberGenerator.GetBytes(32), RandomNumberGenerator.GetBytes(12))
    { }

    public AesCounterMode(byte[] key, byte[] initializationValue)
    {
        if (key.Length != 32)
        {
            throw new CryptographicException("Key must be 32-byte long");
        }

        if (initializationValue.Length != 12)
        {
            throw new CryptographicException("Initialization value must be 12-byte long");
        }

        _aes = Aes.Create();
        Key = key;
        InitializationValue = initializationValue;
        _aes.Mode = CipherMode.ECB;
        _aes.Padding = PaddingMode.None;
        _encryptor = _aes.CreateEncryptor();
        InitializationValue.CopyTo(_counterAndInitializationValue, 4);
        _encryptor.TransformBlock(_counterAndInitializationValue, 0, 16,
            _keyStream, 0);
    }

    public byte[] Key
    {
        get => _aes.Key;
        private init => _aes.Key = value;
    }

    public byte[] InitializationValue { get; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _aes.Dispose();
    }

    public void Transform(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        for (int i = 0; i < source.Length; ++i)
        {
            destination[i] = (byte)(source[i] ^ _keyStream[_keyStreamIdx++]);
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
            fixed (byte* block = _counterAndInitializationValue)
            {
                uint* counter = (uint*)block;
                ++*counter;
            }
        }

        _keyStreamIdx = 0;
        _encryptor.TransformBlock(_counterAndInitializationValue, 0, 16,
            _keyStream, 0);
    }
}
