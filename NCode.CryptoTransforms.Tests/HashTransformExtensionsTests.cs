#region Copyright Preamble

//
//    Copyright @ 2023 NCode Group
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

#endregion

using System;
using System.IO;
using System.Security.Cryptography;
using Xunit;

namespace NCode.CryptoTransforms.Tests;

public class HashTransformExtensionsTests
{
    private readonly Random _random = new Random();

    [Fact]
    public void Sha256_Compute()
    {
        using var platformHasher = SHA256.Create();
        using var hasherToTest = new HashTransform(HashAlgorithmName.SHA256.Name);

        Assert.Equal(platformHasher.HashSize, hasherToTest.HashSize);

        var input = new byte[1024];
        _random.NextBytes(input);

        var hash = hasherToTest.ComputeHash(input);
        var expected = platformHasher.ComputeHash(input);

        Assert.Equal(expected, hash);
    }

    [Fact]
    public void Sha256_ComputeOffset()
    {
        using var platformHasher = SHA256.Create();
        using var hasherToTest = new HashTransform(HashAlgorithmName.SHA256.Name);

        Assert.Equal(platformHasher.HashSize, hasherToTest.HashSize);

        var input = new byte[1024];
        _random.NextBytes(input);

        var offset = _random.Next(100, 200);
        var count = input.Length - offset;

        var hash = hasherToTest.ComputeHash(input, offset, count);
        var expected = platformHasher.ComputeHash(input, offset, count);

        Assert.Equal(expected, hash);
    }

    [Fact]
    public void Sha256_ComputeStream()
    {
        var buffer = new byte[1024];
        _random.NextBytes(buffer);

        using var platformHasher = SHA256.Create();
        using var hasherToTest = new HashTransform(HashAlgorithmName.SHA256.Name);
        using var stream = new MemoryStream(buffer);

        Assert.Equal(platformHasher.HashSize, hasherToTest.HashSize);

        stream.Position = 0;
        var hash = hasherToTest.ComputeHash(stream);

        stream.Position = 0;
        var expected = platformHasher.ComputeHash(stream);

        Assert.Equal(expected, hash);
    }
}