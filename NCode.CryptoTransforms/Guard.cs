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
using System.Runtime.CompilerServices;

namespace NCode.CryptoTransforms
{
    internal static class Guard
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidateTransformBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            if (inputBuffer == null)
                throw new ArgumentNullException(nameof(inputBuffer));

            if (inputOffset < 0)
                throw new ArgumentOutOfRangeException(nameof(inputOffset), "Non-negative number required.");

            if (inputCount < 0 || inputCount > inputBuffer.Length)
                throw new ArgumentException("Value was invalid.");

            if (inputBuffer.Length - inputCount < inputOffset)
                throw new ArgumentException(
                    "Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
        }
    }
}