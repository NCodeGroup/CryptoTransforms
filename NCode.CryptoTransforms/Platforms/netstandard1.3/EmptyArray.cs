using System;

// ReSharper disable once CheckNamespace
namespace NCode.CryptoTransforms
{
    internal static class EmptyArray<T>
    {
        public static T[] Value => Array.Empty<T>();
    }
}