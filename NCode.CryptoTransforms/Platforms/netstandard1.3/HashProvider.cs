using System;
using System.Security.Cryptography;

// ReSharper disable once CheckNamespace
namespace NCode.CryptoTransforms
{
    /// <summary>
    /// Provides the platform specific implementation for <see cref="IHashTransform"/>
    /// using the <c>.NET Standard 1.3</c> API.
    /// </summary>
    internal class HashProvider : IHashTransform
    {
        private readonly IncrementalHash _inner;
        private int? _hashSize;
        private byte[] _hash;
        private bool _disposed;

        public static IHashTransform Create(string hashName)
        {
            return new HashProvider(IncrementalHash.CreateHash(new HashAlgorithmName(hashName)));
        }

        /// <inheritdoc />
        public HashProvider(IncrementalHash inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _disposed) return;
            _inner.Dispose();
            _disposed = true;
        }

        /// <inheritdoc />
        public virtual int HashSize => _hashSize ?? (int)(_hashSize = GetHashSize());

        /// <inheritdoc />
        public virtual byte[] Hash
        {
            get
            {
                var hash = _hash;
                if (_disposed) throw new ObjectDisposedException(null);
                if (hash == null) throw new CryptographicException("Hash must be finalized before the hash value is retrieved.");
                return (byte[])hash.Clone();
            }
        }

        /// <inheritdoc />
        public virtual bool CanReuseTransform => true;

        /// <inheritdoc />
        public virtual bool CanTransformMultipleBlocks => true;

        /// <inheritdoc />
        public virtual int InputBlockSize => 1;

        /// <inheritdoc />
        public virtual int OutputBlockSize => 1;

        /// <inheritdoc />
        public virtual int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if (_disposed) throw new ObjectDisposedException(null);
            Guard.ValidateTransformBlock(inputBuffer, inputOffset, inputCount);

            _inner.AppendData(inputBuffer, inputOffset, inputCount);
            _hash = null;

            if (outputBuffer != null && (inputBuffer != outputBuffer || inputOffset != outputOffset))
                // we let BlockCopy do the destination array validation
                Buffer.BlockCopy(inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);

            return inputCount;
        }

        /// <inheritdoc />
        public virtual byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            if (_disposed) throw new ObjectDisposedException(null);
            Guard.ValidateTransformBlock(inputBuffer, inputOffset, inputCount);

            _inner.AppendData(inputBuffer, inputOffset, inputCount);
            _hash = _inner.GetHashAndReset();

            if (inputCount == 0)
                return Array.Empty<byte>();

            var buffer = new byte[inputCount];
            Buffer.BlockCopy(inputBuffer, inputOffset, buffer, 0, inputCount);
            return buffer;
        }

        private int GetHashSize()
        {
            using (var hasher = IncrementalHash.CreateHash(_inner.AlgorithmName))
            {
                const int bitsPerByte = 8;
                return hasher.GetHashAndReset().Length * bitsPerByte;
            }
        }

    }
}