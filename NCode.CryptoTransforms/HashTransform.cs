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

using System.Security.Cryptography;

namespace NCode.CryptoTransforms
{
    /// <summary>
    /// Represents a cryptographic hash algorithm.
    /// </summary>
    public interface IHashTransform : ICryptoTransform
    {
        /// <summary>
        /// Gets the size, in bits, of the computed hash code.
        /// </summary>
        int HashSize { get; }

        /// <summary>
        /// Gets the value of the computed hash code.
        /// </summary>
        byte[] Hash { get; }
    }

    /// <summary>
    /// Provides the implemenation for a cryptographic hash algorithm.
    /// </summary>
    /// <remarks>
    /// We derive from <see cref="DelegatingHashTransform"/> and not <see cref="HashProvider"/>
    /// so that our API surface is consistent across all platforms.
    /// </remarks>
    public class HashTransform : DelegatingHashTransform
    {
        public HashTransform(string hashName)
            : base(HashProvider.Create(hashName))
        {
            // nothing
        }
    }
}