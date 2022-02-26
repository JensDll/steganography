using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using SixLabors.ImageSharp;

namespace WebApi.Helpers;

public static class FileHelper
{
    private static readonly Dictionary<string, List<byte[]>> _fileSignature = new()
    {
        {
            ".gif",
            new List<byte[]>
            {
                new byte[] {0x47, 0x49, 0x46, 0x38}
            }
        },
        {
            ".png",
            new List<byte[]>
            {
                new byte[] {0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A}
            }
        },
        {
            ".jpeg",
            new List<byte[]>
            {
                new byte[] {0xFF, 0xD8, 0xFF, 0xE0},
                new byte[] {0xFF, 0xD8, 0xFF, 0xE2},
                new byte[] {0xFF, 0xD8, 0xFF, 0xE3}
            }
        },
        {
            ".jpg",
            new List<byte[]>
            {
                new byte[] {0xFF, 0xD8, 0xFF, 0xE0},
                new byte[] {0xFF, 0xD8, 0xFF, 0xE1},
                new byte[] {0xFF, 0xD8, 0xFF, 0xE8}
            }
        }
    };

    public static async Task<Image> ProcessStreamedFile(MultipartSection section,
        ContentDispositionHeaderValue contentDisposition,
        string[] permittedExtensions)
    {
        section.Body!.Position = 0;
        using Image? image = await Image.LoadAsync(section.Body);
        return image;
    }

    private static bool IsValidFileExtensionAndSignature(string fileName, Stream data,
        string[] permittedExtensions)
    {
        if (string.IsNullOrEmpty(fileName) || data.Length == 0)
        {
            return false;
        }

        string extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (string.IsNullOrEmpty(extension) || !permittedExtensions.Contains(extension))
        {
            return false;
        }

        data.Position = 0;

        using BinaryReader reader = new(data);

        List<byte[]> signatures = _fileSignature[extension];
        byte[] headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

        return signatures.Any(signature =>
            headerBytes.Take(signature.Length).SequenceEqual(signature));
    }
}
