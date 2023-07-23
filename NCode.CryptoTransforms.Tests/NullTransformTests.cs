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
using Xunit;

namespace NCode.CryptoTransforms.Tests;

public class NullTransformTests
{
    private readonly Random _random = new Random();

    [Fact]
    public void Properties()
    {
        var transform = new NullTransform();

        Assert.True(transform.CanReuseTransform);
        Assert.True(transform.CanTransformMultipleBlocks);
        Assert.Equal(1, transform.InputBlockSize);
        Assert.Equal(1, transform.OutputBlockSize);
    }

    [Fact]
    public void OutputIsNull()
    {
        var transform = new NullTransform();

        var buffer = new byte[10];
        _random.NextBytes(buffer);

        var clone = (byte[])buffer.Clone();

        var result = transform.TransformBlock(buffer, 0, buffer.Length, null, 0);

        Assert.Equal(buffer.Length, result);
        Assert.Equal(clone, buffer);
    }

    [Fact]
    public void OutputIsInput()
    {
        var transform = new NullTransform();

        var buffer = new byte[10];
        _random.NextBytes(buffer);

        var clone = (byte[])buffer.Clone();

        var result = transform.TransformBlock(buffer, 0, buffer.Length, buffer, 0);

        Assert.Equal(buffer.Length, result);
        Assert.Equal(clone, buffer);
    }

    [Fact]
    public void OutputIsInputOffset()
    {
        var transform = new NullTransform();

        var buffer = new byte[10];
        _random.NextBytes(buffer);

        var clone = (byte[])buffer.Clone();

        var inputCount = 5;
        var outputOffset = 5;
        var result = transform.TransformBlock(buffer, 0, inputCount, buffer, outputOffset);

        Assert.Equal(inputCount, result);

        Buffer.BlockCopy(clone, 0, clone, outputOffset, inputCount);
        Assert.Equal(clone, buffer);
    }

    [Fact]
    public void InputOutput()
    {
        var transform = new NullTransform();

        var input = new byte[10];
        _random.NextBytes(input);

        var output = new byte[10];
        _random.NextBytes(output);

        Assert.NotEqual(input, output);

        var result = transform.TransformBlock(input, 0, input.Length, output, 0);

        Assert.Equal(input.Length, result);
        Assert.Equal(input, output);
    }
}