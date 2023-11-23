﻿using System.Buffers;
using NUnit.Framework;

namespace Domain.Test;

[TestFixture]
internal sealed class RandomExtensionsTests
{
#pragma warning disable CA1861
    [TestCase(0, 10, 5, new[] { 0, 5, 10 })]
    [TestCase(1, 10, 5, new[] { 1, 6 })]
    [TestCase(3, 31, 9, new[] { 3, 12, 21, 30 })]
    [TestCase(2, 18, 6, new[] { 2, 8, 14 })]
    [TestCase(3, 18, 7, new[] { 3, 10, 17 })]
    [TestCase(4, 18, 7, new[] { 4, 11, 18 })]
    [TestCase(5, 18, 7, new[] { 5, 12 })]
    [TestCase(6, 18, 7, new[] { 6, 13 })]
    [TestCase(7, 18, 7, new[] { 7, 14 })]
#pragma warning restore CA1861
    public void RentPermutation_Should_Generate_Rented_Permutation(int start, int end, int step, int[] expectedValues)
    {
        // Act
        (int[] permutation, int count) = Random.Shared.RentPermutation(start, end, step);

        // Assert
        Assert.That(count, Is.EqualTo(expectedValues.Length));
        Assert.That(permutation[..count], Is.EquivalentTo(expectedValues));

        ArrayPool<int>.Shared.Return(permutation);
    }

    [Test]
    public void RentPermutation_Should_Throw_ArgumentException_When_Start_Is_Greater_Than_End()
    {
        // Assert
        Assert.That(static () => /* Act */ Random.Shared.RentPermutation(10, 5), Throws.ArgumentException);
    }
}
