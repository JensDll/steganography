using System;
using System.Text;
using Domain.Entities;
using NUnit.Framework;

namespace Domain.UnitTests.Entities;

[TestFixture]
internal class ParsingUtilsTests
{
    [Test]
    public void CopyAsInt32([Random(0, int.MaxValue, 10)] int actual)
    {
        // Arrange
        ReadOnlySpan<byte> number = Encoding.ASCII.GetBytes(actual.ToString());
        Span<byte> destination = new byte[4];

        // Act
        ParsingUtils.CopyAsInt32(number, destination);

        // Assert
        int result = BitConverter.ToInt32(destination);
        Assert.That(actual, Is.EqualTo(result));
    }
}
