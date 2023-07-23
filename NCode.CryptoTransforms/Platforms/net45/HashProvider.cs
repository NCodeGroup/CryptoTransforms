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
using System.Security.Cryptography;

namespace NCode.CryptoTransforms;

/// <summary>
/// Provides the platform specific implementation for <see cref="IHashTransform"/>
/// using the <c>.NET Framework 4.5</c> API.
/// </summary>
internal class HashProvider : DelegatingCryptoTransform, IHashTransform
{
    private readonly HashAlgorithm _inner;
    private byte[] _hash;
    private bool _disposed;

    public static IHashTransform Create(string hashName)
    {
        return new HashProvider(HashAlgorithm.Create(hashName));
    }

    /// <inheritdoc />
    public HashProvider(HashAlgorithm inner)
        : base(inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        _disposed = _disposed || disposing;
        // inner will be disposed via base class
        base.Dispose(disposing);
    }

    /// <inheritdoc />
    public virtual int HashSize => _inner.HashSize;

    /// <inheritdoc />
    public virtual byte[] Hash
    {
        get
        {
            var hash = _hash;
            if (_disposed) throw new ObjectDisposedException(null);
            if (hash == null)
                throw new CryptographicUnexpectedOperationException(
                    "Hash must be finalized before the hash value is retrieved.");
            return (byte[])hash.Clone();
        }
    }

    /// <inheritdoc />
    public override int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer,
        int outputOffset)
    {
        if (_disposed) throw new ObjectDisposedException(null);

        _hash = null;
        return base.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
    }

    /// <inheritdoc />
    public override byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
    {
        if (_disposed) throw new ObjectDisposedException(null);

        // we let the inner provider do the array validation
        var result = base.TransformFinalBlock(inputBuffer, inputOffset, inputCount);

        // stamp the hash value
        _hash = _inner.Hash;

        // re-initialize so that the hash algorithm can be re-used
        _inner.Initialize();

        return result;
    }
}