#region Copyright Preamble

// 
//    Copyright @ 2017 NCode Group
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
using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using NCode.ArrayLeases;

namespace NCode.CryptoTransforms
{
    public enum ToBase64TransformMode
    {
        Default = 0,
        InsertNewLines = 1
    }

    public class ToBase64Transform : ICryptoTransform
    {
        private const int CharsPerLine = 76;
        private readonly ArrayPool<byte> _poolBytes;
        private readonly ArrayPool<char> _poolChars;

        public ToBase64Transform()
            : this(null, null)
        {
            // nothing
        }

        public ToBase64Transform(ArrayPool<byte> poolBytes, ArrayPool<char> poolChars)
        {
            _poolBytes = poolBytes ?? ArrayPool<byte>.Shared;
            _poolChars = poolChars ?? ArrayPool<char>.Shared;
        }

        public bool CanReuseTransform => true;
        public bool CanTransformMultipleBlocks => true;

        public int InputBlockSize => 3;
        public int OutputBlockSize => 4;

        public void Dispose()
        {
            // nothing
        }

        public virtual int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer,
            int outputOffset)
        {
            Guard.ValidateTransformBlock(inputBuffer, inputOffset, inputCount);

            var numChars = DetermineNumCharsForNumBytes(inputCount);
            using (var leaseChars = _poolChars.Lease(numChars))
            using (var leaseBytes = _poolBytes.Lease(numChars))
            {
                var numCharsActual =
                    Convert.ToBase64CharArray(inputBuffer, inputOffset, inputCount, leaseChars.Array, 0);
                if (numCharsActual != numChars)
                    throw new CryptographicException("Length of the data to encrypt is invalid.");

                var numBytes = Encoding.ASCII.GetBytes(leaseChars.Array, 0, numChars, leaseBytes.Array, 0);
                if (numBytes != numChars)
                    throw new CryptographicException("Length of the data to encrypt is invalid.");

                Buffer.BlockCopy(leaseBytes.Array, 0, outputBuffer, outputOffset, numBytes);
                return numBytes;
            }
        }

        public virtual byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            Guard.ValidateTransformBlock(inputBuffer, inputOffset, inputCount);

            if (inputCount == 0)
                return EmptyArray<byte>.Value;

            var numChars = DetermineNumCharsForNumBytes(inputCount);
            using (var leaseChars = _poolChars.Lease(numChars))
            {
                var numCharsActual =
                    Convert.ToBase64CharArray(inputBuffer, inputOffset, inputCount, leaseChars.Array, 0);
                if (numCharsActual != numChars)
                    throw new CryptographicException("Length of the data to encrypt is invalid.");

                var bytes = Encoding.ASCII.GetBytes(leaseChars.Array, 0, numChars);
                if (bytes.Length != numChars)
                    throw new CryptographicException("Length of the data to encrypt is invalid.");

                return bytes;
            }
        }

        public static int DetermineNumCharsForNumBytes(int numBytes)
        {
            // every 3 bytes is encoded into 4 chars rounded up to multiples of 4 chars
            return (4 * numBytes / 3 + 3) & ~3;
        }
    }
}