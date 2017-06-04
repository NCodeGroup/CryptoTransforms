using System;
using System.IO;
using System.Security.Cryptography;
using Xunit;

namespace NCode.CryptoTransforms.Tests
{
    public class HashTransformExtensionsTests
    {
        private readonly Random _random = new Random();

        [Fact]
        public void Sha256_Compute()
        {
            using (var platformHasher = SHA256.Create())
            using (var hasherToTest = new HashTransform(HashAlgorithmName.SHA256.Name))
            {
                Assert.Equal(platformHasher.HashSize, hasherToTest.HashSize);

                var input = new byte[1024];
                _random.NextBytes(input);

                var hash = hasherToTest.ComputeHash(input);
                var expected = platformHasher.ComputeHash(input);

                Assert.Equal(expected, hash);
            }
        }

        [Fact]
        public void Sha256_ComputeOffset()
        {
            using (var platformHasher = SHA256.Create())
            using (var hasherToTest = new HashTransform(HashAlgorithmName.SHA256.Name))
            {
                Assert.Equal(platformHasher.HashSize, hasherToTest.HashSize);

                var input = new byte[1024];
                _random.NextBytes(input);

                var offset = _random.Next(100, 200);
                var count = input.Length - offset;

                var hash = hasherToTest.ComputeHash(input, offset, count);
                var expected = platformHasher.ComputeHash(input, offset, count);

                Assert.Equal(expected, hash);
            }
        }

        [Fact]
        public void Sha256_ComputeStream()
        {
            var buffer = new byte[1024];
            _random.NextBytes(buffer);

            using (var platformHasher = SHA256.Create())
            using (var hasherToTest = new HashTransform(HashAlgorithmName.SHA256.Name))
            using (var stream = new MemoryStream(buffer))
            {
                Assert.Equal(platformHasher.HashSize, hasherToTest.HashSize);

                stream.Position = 0;
                var hash = hasherToTest.ComputeHash(stream);

                stream.Position = 0;
                var expected = platformHasher.ComputeHash(stream);

                Assert.Equal(expected, hash);
            }
        }

    }
}