using System;
using System.Security.Cryptography;
using Xunit;

namespace NCode.CryptoTransforms.Tests
{
    public class HashTransformTests
    {
        private readonly Random _random = new Random();

        [Fact]
        public void Sha256_TransformFinalBlock()
        {
            using (var platformHasher = SHA256.Create())
            using (var hasherToTest = new HashTransform(HashAlgorithmName.SHA256.Name))
            {
                Assert.Equal(platformHasher.HashSize, hasherToTest.HashSize);

                var input = new byte[1024];
                _random.NextBytes(input);

                var output = hasherToTest.TransformFinalBlock(input, 0, input.Length);
                Assert.Equal(input, output);

                var expected = platformHasher.ComputeHash(input);
                var hash = hasherToTest.Hash;
                Assert.Equal(expected, hash);
            }
        }

        [Fact]
        public void Sha256_TransformBlocks()
        {
            using (var platformHasher = SHA256.Create())
            using (var hasherToTest = new HashTransform(HashAlgorithmName.SHA256.Name))
            {
                Assert.Equal(platformHasher.HashSize, hasherToTest.HashSize);

                var input = new byte[1024];
                _random.NextBytes(input);

                var inputOffset = 0;
                var remainingCount = input.Length;

                while (remainingCount > 0)
                {
                    // read random segments of any length between 10-20
                    var inputCount = Math.Min(remainingCount, _random.Next(10, 20));

                    var check = new byte[inputCount];
                    Buffer.BlockCopy(input, inputOffset, check, 0, inputCount);

                    var outputBuffer = new byte[inputCount];

                    var result = hasherToTest.TransformBlock(input, inputOffset, inputCount, outputBuffer, 0);

                    Assert.Equal(inputCount, result);
                    Assert.Equal(check, outputBuffer);

                    remainingCount -= result;
                    inputOffset += result;
                }

                var outputFinal = hasherToTest.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                Assert.Equal(0, outputFinal.Length);

                var expected = platformHasher.ComputeHash(input);
                var hash = hasherToTest.Hash;
                Assert.Equal(expected, hash);
            }
        }

    }
}