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
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace NCode.CryptoTransforms.Tests;

public class Base64TransformTests
{
    public static IEnumerable<object[]> TestData_Ascii()
    {
        // Test data taken from RFC 4648 Test Vectors
        yield return new object[] { "", "" };
        yield return new object[] { "f", "Zg==" };
        yield return new object[] { "fo", "Zm8=" };
        yield return new object[] { "foo", "Zm9v" };
        yield return new object[] { "foob", "Zm9vYg==" };
        yield return new object[] { "fooba", "Zm9vYmE=" };
        yield return new object[] { "foobar", "Zm9vYmFy" };
    }

    public static IEnumerable<object[]> TestData_Ascii_Whitespace()
    {
        yield return new object[] { "fo", "\tZ\tm8=\n" };
        yield return new object[] { "foo", " Z m 9 v" };
    }

    [Theory]
    [MemberData(nameof(TestData_Ascii))]
    public static void ValidateToBase64CryptoStream(string data, string encoding)
    {
        using var transform = new ToBase64Transform();

        ValidateCryptoStream(encoding, data, transform);
    }

    [Theory]
    [MemberData(nameof(TestData_Ascii))]
    public static void ValidateFromBase64CryptoStream(string data, string encoding)
    {
        using var transform = new FromBase64Transform();

        ValidateCryptoStream(data, encoding, transform);
    }

    private static void ValidateCryptoStream(string expected, string data, ICryptoTransform transform)
    {
        var inputBytes = Encoding.ASCII.GetBytes(data);
        var outputBytes = new byte[100];

        using var ms = new MemoryStream(inputBytes);
        using var cs = new CryptoStream(ms, transform, CryptoStreamMode.Read);

        var totalBytes = 0;
        int bytesRead;
        do
        {
            bytesRead = cs.Read(outputBytes, totalBytes, outputBytes.Length - totalBytes);
            totalBytes += bytesRead;
        } while (bytesRead > 0);

        var outputString = Encoding.ASCII.GetString(outputBytes, 0, totalBytes);
        Assert.Equal(expected, outputString);
    }

    [Theory]
    [MemberData(nameof(TestData_Ascii))]
    public void ValidateToBase64TransformBlock(string data, string expected)
    {
        var inputBytes = Encoding.ASCII.GetBytes(data);
        var outputBytes = new byte[100];

        using var transform = new ToBase64Transform();

        var numChars = transform.TransformBlock(inputBytes, 0, inputBytes.Length, outputBytes, 0);
        transform.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

        var outputString = Encoding.ASCII.GetString(outputBytes, 0, numChars);
        Assert.Equal(expected, outputString);
    }

    [Theory]
    [MemberData(nameof(TestData_Ascii))]
    public void ValidateToBase64TransformFinalBlock(string data, string expected)
    {
        var inputBytes = Encoding.ASCII.GetBytes(data);

        using var transform = new ToBase64Transform();

        var outputBytes = transform.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
        var outputString = Encoding.ASCII.GetString(outputBytes);
        Assert.Equal(expected, outputString);
    }

    [Theory]
    [MemberData(nameof(TestData_Ascii))]
    public void ValidateFromBase64TransformBlock(string expected, string encoded)
    {
        var inputBytes = Encoding.ASCII.GetBytes(encoded);
        var outputBytes = new byte[100];

        using var transform = new FromBase64Transform();

        var numChars = transform.TransformBlock(inputBytes, 0, inputBytes.Length, outputBytes, 0);
        transform.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

        var outputString = Encoding.ASCII.GetString(outputBytes, 0, numChars);
        Assert.Equal(expected, outputString);
    }

    [Theory]
    [MemberData(nameof(TestData_Ascii))]
    public void ValidateFromBase64TransformFinalBlock(string expected, string encoded)
    {
        var inputBytes = Encoding.ASCII.GetBytes(encoded);

        using var transform = new FromBase64Transform();

        var outputBytes = transform.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
        var outputString = Encoding.ASCII.GetString(outputBytes);
        Assert.Equal(expected, outputString);
    }

    [Theory]
    [MemberData(nameof(TestData_Ascii_Whitespace))]
    public static void ValidateWhitespace(string expected, string data)
    {
        var inputBytes = Encoding.ASCII.GetBytes(data);
        var outputBytes = new byte[100];

        // Verify default of FromBase64TransformMode.IgnoreWhiteSpaces
        using (var base64Transform = new FromBase64Transform())
        using (var ms = new MemoryStream(inputBytes))
        using (var cs = new CryptoStream(ms, base64Transform, CryptoStreamMode.Read))
        {
            var bytesRead = cs.Read(outputBytes, 0, outputBytes.Length);
            var outputString = Encoding.ASCII.GetString(outputBytes, 0, bytesRead);
            Assert.Equal(expected, outputString);
        }

        // Verify explicit FromBase64TransformMode.IgnoreWhiteSpaces
        // ReSharper disable once RedundantArgumentDefaultValue
        using (var base64Transform = new FromBase64Transform(FromBase64TransformMode.IgnoreWhiteSpaces))
        using (var ms = new MemoryStream(inputBytes))
        using (var cs = new CryptoStream(ms, base64Transform, CryptoStreamMode.Read))
        {
            var bytesRead = cs.Read(outputBytes, 0, outputBytes.Length);
            var outputString = Encoding.ASCII.GetString(outputBytes, 0, bytesRead);
            Assert.Equal(expected, outputString);
        }
    }

    [Fact]
    public void ValidateNumCharsForNumBytes()
    {
        Assert.Equal(0, ToBase64Transform.DetermineNumCharsForNumBytes(0));

        Assert.Equal(4, ToBase64Transform.DetermineNumCharsForNumBytes(1));
        Assert.Equal(4, ToBase64Transform.DetermineNumCharsForNumBytes(2));
        Assert.Equal(4, ToBase64Transform.DetermineNumCharsForNumBytes(3));

        Assert.Equal(8, ToBase64Transform.DetermineNumCharsForNumBytes(4));
        Assert.Equal(8, ToBase64Transform.DetermineNumCharsForNumBytes(5));
        Assert.Equal(8, ToBase64Transform.DetermineNumCharsForNumBytes(6));
    }
}