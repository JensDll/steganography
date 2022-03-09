﻿using Domain.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Domain.Interfaces;

public interface IDecoder
{
    public byte[] Decode(Image<Rgb24> coverImage, ushort seed, int messageLength);

    public List<DecodedItem> ParseMessage(ReadOnlySpan<byte> message, out bool isText);
}