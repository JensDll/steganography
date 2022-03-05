using System;
using Domain.Extensions;
using NUnit.Framework;

namespace Domain.UnitTests.Extensions;

[TestFixture]
internal class RandomExtensionsTests
{
    [TestCase(0, 10, 5, new[] {0, 5, 10})]
    [TestCase(1, 10, 5, new[] {1, 6})]
    [TestCase(2, 18, 6, new[] {2, 8, 14})]
    [TestCase(3, 18, 7, new[] {3, 10, 17})]
    [TestCase(4, 18, 7, new[] {4, 11, 18})]
    [TestCase(5, 18, 7, new[] {5, 12})]
    [TestCase(6, 18, 7, new[] {6, 13})]
    [TestCase(7, 18, 7, new[] {7, 14})]
    public void Permutation_ShouldGeneratePermutation(int start, int end, int step, int[] expectedValues)
    {
        // Arrange
        Random prng = new();

        // Act
        int[] permutation = prng.Permutation(start, end, step);

        // Assert
        Assert.That(permutation, Is.EquivalentTo(expectedValues));
    }

    [Test]
    public void Permutation_ShouldThrowArgumentException_WhenStartIsGreaterThanEnd()
    {
        // Arrange
        Random prng = new();

        // Act
        void Action()
        {
            prng.Permutation(10, 5, 1);
        }

        // Assert
        Assert.That(Action, Throws.ArgumentException);
    }
}
