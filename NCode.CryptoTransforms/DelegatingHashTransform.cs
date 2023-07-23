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

namespace NCode.CryptoTransforms;

/// <summary>
/// Forwards all calls to an inner <see cref="IHashTransform"/> except where
/// overridden in a derived class.
/// </summary>
public class DelegatingHashTransform : DelegatingCryptoTransform, IHashTransform
{
    private readonly IHashTransform _inner;

    /// <summary>
    /// Initializes a new instance of the <see cref="DelegatingHashTransform"/> class.
    /// </summary>
    /// <param name="inner">The <see cref="IHashTransform"/> to delegate all calls to.</param>
    public DelegatingHashTransform(IHashTransform inner)
        : base(inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    /// <inheritdoc />
    public virtual int HashSize => _inner.HashSize;

    /// <inheritdoc />
    public virtual byte[] Hash => _inner.Hash;
}