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
    /// Forwards all calls to an inner <see cref="ICryptoTransform"/> except
    /// where overridden in a derived class.
    /// </summary>
    public class DelegatingCryptoTransform : ICryptoTransform
    {
        private readonly ICryptoTransform _inner;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegatingCryptoTransform"/> class.
        /// </summary>
        /// <param name="inner">The <see cref="ICryptoTransform"/> to delegate all calls to.</param>
        public DelegatingCryptoTransform(ICryptoTransform inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public virtual bool CanReuseTransform => _inner.CanReuseTransform;

        /// <inheritdoc />
        public virtual bool CanTransformMultipleBlocks => _inner.CanTransformMultipleBlocks;

        /// <inheritdoc />
        public virtual int InputBlockSize => _inner.InputBlockSize;

        /// <inheritdoc />
        public virtual int OutputBlockSize => _inner.OutputBlockSize;

        /// <inheritdoc />
        public virtual int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer,
            int outputOffset) => _inner.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer,
            outputOffset);

        /// <inheritdoc />
        public virtual byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount) => _inner
            .TransformFinalBlock(inputBuffer, inputOffset, inputCount);

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DelegatingCryptoTransform"/>
        /// class and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged
        /// resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing) _inner.Dispose();
        }
    }
}