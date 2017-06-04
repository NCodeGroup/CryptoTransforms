namespace NCode.CryptoTransforms
{
    internal static class EmptyArray<T>
    {
        public static T[] Value { get; } = new T[0];
    }
}