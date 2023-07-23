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
using System.Buffers;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using NCode.ArrayLeases;

namespace NCode.CryptoTransforms;

/// <summary>
/// Specifies whether white space should be ignored in the Base 64 transformation.
/// </summary>
public enum FromBase64TransformMode
{
    /// <summary>
    /// Specifies that white space should be ignored.
    /// </summary>
    IgnoreWhiteSpaces = 0,

    /// <summary>
    /// Specifies that white space should not be ignored.
    /// </summary>
    DoNotIgnoreWhiteSpaces = 1
}

/// <summary>
/// Converts a <see cref="CryptoStream"/> from Base 64.
/// </summary>
public class FromBase64Transform : ICryptoTransform
{
    private readonly ArrayPool<byte> _poolBytes;
    private readonly FromBase64TransformMode _whitespaces;
    private byte[] _workingBuffer = new byte[4];
    private int _workingIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="FromBase64Transform"/> class with the specified transformation mode.
    /// </summary>
    /// <param name="whitespaces">One of the <see cref="FromBase64TransformMode"/> values.</param>
    public FromBase64Transform(FromBase64TransformMode whitespaces = FromBase64TransformMode.IgnoreWhiteSpaces)
        : this(null, whitespaces)
    {
        // nothing
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FromBase64Transform"/> class with the specified transformation mode and array byte pool.
    /// </summary>
    /// <param name="poolBytes">An <see cref="ArrayPool{T}"/> from where to allocate byte arrays.</param>
    /// <param name="whitespaces">One of the <see cref="FromBase64TransformMode"/> values.</param>
    public FromBase64Transform(
        ArrayPool<byte> poolBytes,
        FromBase64TransformMode whitespaces = FromBase64TransformMode.IgnoreWhiteSpaces)
    {
        _poolBytes = poolBytes ?? ArrayPool<byte>.Shared;
        _whitespaces = whitespaces;
    }

    /// <inheritdoc />
    public int InputBlockSize => 1;

    /// <inheritdoc />
    public int OutputBlockSize => 3;

    /// <inheritdoc />
    public bool CanReuseTransform => true;

    /// <inheritdoc />
    public bool CanTransformMultipleBlocks => true;

    /// <inheritdoc />
    public virtual int TransformBlock(
        byte[] inputBuffer,
        int inputOffset,
        int inputCount,
        byte[] outputBuffer,
        int outputOffset)
    {
        Guard.ValidateTransformBlock(inputBuffer, inputOffset, inputCount);
        if (_workingBuffer == null) throw new ObjectDisposedException(GetType().FullName);

        using var tempBuffer = GetTempBuffer(inputBuffer, inputOffset, inputCount);

        var bufferLen = tempBuffer.Count;
        if (bufferLen + _workingIndex < 4)
        {
            Buffer.BlockCopy(tempBuffer.Array, 0, _workingBuffer, _workingIndex, bufferLen);
            _workingIndex += bufferLen;
            return 0;
        }

        var bytes = ConvertFromBase64(tempBuffer);
        Buffer.BlockCopy(bytes, 0, outputBuffer, outputOffset, bytes.Length);
        return bytes.Length;
    }

    /// <inheritdoc />
    public virtual byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
    {
        Guard.ValidateTransformBlock(inputBuffer, inputOffset, inputCount);
        if (_workingBuffer == null) throw new ObjectDisposedException(GetType().FullName);

        var bytes = EmptyArray<byte>.Value;
        using (var tempBuffer = GetTempBuffer(inputBuffer, inputOffset, inputCount))
        {
            if (tempBuffer.Count + _workingIndex >= 4)
                bytes = ConvertFromBase64(tempBuffer);
        }

        _workingIndex = 0;

        return bytes;
    }

    private IArrayLease<byte> GetTempBuffer(byte[] inputBuffer, int inputOffset, int inputCount)
    {
        IArrayLease<byte> tempBuffer = null;
        try
        {
            if (inputCount == 0)
            {
                tempBuffer = EmptyArray<byte>.Value.Lease();
            }
            else if (_whitespaces == FromBase64TransformMode.IgnoreWhiteSpaces)
            {
                tempBuffer = DiscardWhiteSpaces(inputBuffer, inputOffset, inputCount);
            }
            else if (inputOffset == 0)
            {
                tempBuffer = inputBuffer.Lease(inputCount);
            }
            else
            {
                tempBuffer = _poolBytes.Lease(inputCount);
                Buffer.BlockCopy(inputBuffer, inputOffset, tempBuffer.Array, 0, inputCount);
            }

            return tempBuffer;
        }
        catch
        {
            tempBuffer?.Dispose();
            throw;
        }
    }

    private byte[] ConvertFromBase64(IArrayLease<byte> tempBuffer)
    {
        var bufferLen = tempBuffer.Count;
        var numBlocks = (bufferLen + _workingIndex) / 4;

        using var transformBuffer = _poolBytes.Lease(_workingIndex + bufferLen);

        Buffer.BlockCopy(_workingBuffer, 0, transformBuffer.Array, 0, _workingIndex);
        Buffer.BlockCopy(tempBuffer.Array, 0, transformBuffer.Array, _workingIndex, bufferLen);

        _workingIndex = (bufferLen + _workingIndex) % 4;
        Buffer.BlockCopy(tempBuffer.Array, bufferLen - _workingIndex, _workingBuffer, 0, _workingIndex);

        var base64 = Encoding.ASCII.GetChars(transformBuffer.Array, 0, 4 * numBlocks);
        var bytes = Convert.FromBase64CharArray(base64, 0, 4 * numBlocks);
        return bytes;
    }

    private IArrayLease<byte> DiscardWhiteSpaces(byte[] inputBuffer, int inputOffset, int inputCount)
    {
        IArrayLease<byte> lease = null;
        try
        {
            var chCount = 0;
            for (var i = 0; i < inputCount; i++)
            {
                var ch = inputBuffer[inputOffset + i];
                if (char.IsWhiteSpace((char)ch))
                {
                    lease ??= _poolBytes.Lease(inputCount);
                }
                else if (lease != null)
                {
                    lease.Array[chCount++] = ch;
                }
            }

            if (lease == null)
            {
                Debug.Assert(chCount == 0);

                if (inputOffset == 0)
                    return inputBuffer.Lease(inputCount);

                lease = _poolBytes.Lease(inputCount);
                Buffer.BlockCopy(inputBuffer, inputOffset, lease.Array, 0, inputCount);
                return lease;
            }

            Debug.Assert(chCount > 0);
            Debug.Assert(chCount <= inputCount);

            lease.Count = chCount;
            return lease;
        }
        catch
        {
            lease?.Dispose();
            throw;
        }
    }

    #region IDisposable Members

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="FromBase64Transform"/>.
    /// </summary>
    ~FromBase64Transform()
    {
        Dispose(false);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="FromBase64Transform"/>
    /// class and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged
    /// resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;

        var buffer = Interlocked.Exchange(ref _workingBuffer, null);
        if (buffer != null)
            Array.Clear(buffer, 0, buffer.Length);

        _workingIndex = 0;
    }

    #endregion
}