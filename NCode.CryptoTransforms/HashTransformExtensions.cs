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
using System.IO;
using NCode.ArrayLeases;

namespace NCode.CryptoTransforms
{
    public static class HashTransformExtensions
    {
        // Copied from: coreclr/src/mscorlib/src/System/IO/Stream.cs
        // We pick a value that is the largest multiple of 4096 that is still smaller than the large object heap threshold (85K).
        // The CopyTo/CopyToAsync buffer is short-lived and is likely to be collected at Gen0, and it offers a significant
        // improvement in Copy performance.
        private const int DefaultCopyBufferSize = 81920;

        public static byte[] ComputeHash(this IHashTransform transform, byte[] inputBuffer)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));
            if (inputBuffer == null)
                throw new ArgumentNullException(nameof(inputBuffer));

            return transform.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
        }

        public static byte[] ComputeHash(this IHashTransform transform, byte[] inputBuffer, int inputOffset,
            int inputCount)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));
            if (inputBuffer == null)
                throw new ArgumentNullException(nameof(inputBuffer));

            Guard.ValidateTransformBlock(inputBuffer, inputOffset, inputCount);

            return transform.TransformFinalBlock(inputBuffer, inputOffset, inputCount);
        }

        public static byte[] ComputeHash(this IHashTransform transform, Stream stream)
        {
            return ComputeHash(transform, stream, DefaultCopyBufferSize);
        }

        public static byte[] ComputeHash(this IHashTransform transform, Stream stream, int bufferSize)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize), "Positive number required.");

            using (var lease = ArrayPool<byte>.Shared.Lease(bufferSize))
            {
                var buffer = lease.Array;
                int read;
                do
                {
                    read = stream.Read(buffer, 0, buffer.Length);
                    if (read > 0)
                        transform.TransformBlock(buffer, 0, read, null, 0);
                } while (read > 0);
            }
            transform.TransformFinalBlock(EmptyArray<byte>.Value, 0, 0);

            return transform.Hash;
        }
    }
}