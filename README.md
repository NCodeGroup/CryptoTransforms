[![ci](https://github.com/NCodeGroup/CryptoTransforms/actions/workflows/main.yml/badge.svg)](https://github.com/NCodeGroup/CryptoTransforms/actions)

# Overview

This library provides adapters for the missing hashing and base 64 algorithms in the .NET Standard frameworks.
Specifically this library provides implementations of `ICryptoTransform` for `HashAlgorithm`, `ToBase64Transform`,
and `FromBase64Transform`.

## Problem Statement

### Hashing

`HashAlgorithm` has been available for quite some time as a convenient way to abstract various hashing algorithms such
as `MD5` and `SHA256`. It is especially useful in conjunction with `CryptoStream` that allows you to calculate the hash
from a `Stream` while processing its data.

```csharp
Stream s = GetStreamFromSomewhere();
 
using (HashAlgorithm hasher = HashAlgorithm.Create("SHA256"))
{
  using (CryptoStream cs = new CryptoStream(s, hasher, CryptoStreamMode.Read) 
  {
    int byteCount;
    byte[] data = new byte[4096];
    while ((byteCount = cs.Read(data, 0, data.Length)) > 0)
    {
      // do something useful with the actual read data
    }

    byte[] hash = hasher.Hash;
    // do something useful with the hash
  }
}
```

Unfortunatly the definition of `HashAlgorithm` is not consistent across the different .NET Core frameworks:

* [.NET Core 1.0.0 - HashAlgorithm : IDisposable]
* [.NET Core 1.1.0 - HashAlgorithm : IDisposable]
* [.NET Core 2.0.0 - HashAlgorithm : ICryptoTransform]

This means that `HashAlgorithm` cannot be used with `CryptoStream` unless targeting __.NET Standard 2.0__ which hasn't
received much adoption yet.

### Base 64

```
ToBase64Transform : ICryptoTransform
FromBase64Transform : ICryptoTransform
```

Similarly, if you search for `ToBase64Transform` and `FromBase64Transform` using [.NET API Browser] you will find that
these implementations are only available in __.NET Standard 2.0__ which hasn't received much adoption yet.

This means that any developers wishing to target ealier versions of .NET Standard such as 1.3 cannot use these base 64
transforms with `ICryptoTransform` or `CryptoStream`.

## Solution

This library provides the following features:

* An [adapter] implementation of `ICryptoTransform` for the hashing algorithms already available in [.NET Standard]
* An [adapter] implementation of `ICryptoTransform` for implementations of `ToBase64Transform` and `FromBase64Transform`
  using the base 64 libraries already available in [.NET Standard]

## Adapter Details

### Hashing

The following interface and class are provided to represent a cryptographic hash algorithm.

```csharp
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
public class HashTransform : IHashTransform
{
    /// <summary>
    /// Creates an instance of the specified implementation of a hash algorithm.
    /// </summary>
    /// <param name="hashName">The hash algorithm implementation to use.</param>
    public HashTransform(string hashName) { /* ... */ }

    // .NET Core implementation is provided by 'IncrementalHash'
    // .NET Framework implementation is delegated to the existing 'HashAlgorithm' class
}
```

### Base 64

```csharp
public class ToBase64Transform : ICryptoTransform { /* */ }
public class FromBase64Transform : ICryptoTransform { /* */ }
```

These classes simply implement the `ICryptoTransform` interface by using the already
existing `Convert.FromBase64CharArray` and `Convert.ToBase64CharArray` builtin methods.

## Release Notes

* v1.0.0 - Initial release
* v1.0.1 - Refresh the build and add CI using GitHub actions

## Feedback

Please provide any feedback, comments, or issues to this GitHub project [here][issues].

[adapter]: https://en.wikipedia.org/wiki/Adapter_pattern

[issues]: https://github.com/NCodeGroup/NCode.CryptoTransforms/issues

[.NET Standard]: https://docs.microsoft.com/en-us/dotnet/standard/library

[.NET Standard 2.0]: https://github.com/dotnet/standard/blob/master/docs/netstandard-20/README.md

[.NET API Browser]: https://docs.microsoft.com/en-us/dotnet/api/

[.NET Core 1.0.0 - HashAlgorithm : IDisposable]: https://github.com/dotnet/corefx/blob/release/1.0.0/src/System.Security.Cryptography.Primitives/src/System/Security/Cryptography/HashAlgorithm.cs

[.NET Core 1.1.0 - HashAlgorithm : IDisposable]: https://github.com/dotnet/corefx/blob/release/1.1.0/src/System.Security.Cryptography.Primitives/src/System/Security/Cryptography/HashAlgorithm.cs

[.NET Core 2.0.0 - HashAlgorithm : ICryptoTransform]: https://github.com/dotnet/corefx/blob/release/2.0.0/src/System.Security.Cryptography.Primitives/src/System/Security/Cryptography/HashAlgorithm.cs
