using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Domain.Entities;

public class AesCounterMode : IDisposable
{
    private readonly Aes _aes;
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
        IV.CopyTo(_iVAndCounter, 0);
        _aes.EncryptEcb(_iVAndCounter, _keyStream, PaddingMode.None);
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
        iV.CopyTo(_iVAndCounter, 0);
        _aes.EncryptEcb(_iVAndCounter, _keyStream, PaddingMode.None);
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
    private unsafe void GenerateNewKeyStream()
    {
        fixed (byte* bytePtr = &_iVAndCounter[12])
        {
            uint* counter = (uint*) bytePtr;
            ++*counter;
        }

        _keyStreamIdx = 0;
        _aes.EncryptEcb(_iVAndCounter, _keyStream, PaddingMode.None);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _aes.Dispose();
    }
}
