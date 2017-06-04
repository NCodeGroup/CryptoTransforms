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
using System.Security.Cryptography;

namespace NCode.CryptoTransforms
{
    /// <summary>
    /// Provides the implementation for an <see cref="ICryptoTransform"/> that
    /// transforms the input by simply copying the bytes to the output without
    /// any modifications.
    /// </summary>
    public class NullTransform : ICryptoTransform
    {
        /// <inheritdoc />
        public virtual bool CanReuseTransform => true;

        /// <inheritdoc />
        public virtual bool CanTransformMultipleBlocks => true;

        /// <inheritdoc />
        public virtual int InputBlockSize => 1;

        /// <inheritdoc />
        public virtual int OutputBlockSize => 1;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public virtual int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer,
            int outputOffset)
        {
            Guard.ValidateTransformBlock(inputBuffer, inputOffset, inputCount);

            if (outputBuffer != null && (inputBuffer != outputBuffer || inputOffset != outputOffset))
                Buffer.BlockCopy(inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);

            return inputCount;
        }

        /// <inheritdoc />
        public virtual byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            Guard.ValidateTransformBlock(inputBuffer, inputOffset, inputCount);

            if (inputCount == 0) return EmptyArray<byte>.Value;

            var buffer = new byte[inputCount];
            Buffer.BlockCopy(inputBuffer, inputOffset, buffer, 0, inputCount);
            return buffer;
        }

        protected virtual void Dispose(bool disposing)
        {
            // nothing
        }
    }
}