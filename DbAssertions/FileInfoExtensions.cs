using System.IO;

namespace DbAssertions
{
    /// <summary>
    /// FileInfo拡張メソッドクラス
    /// </summary>
    internal static class FileInfoExtensions
    {
        internal static byte[] ReadAllBytes(this FileInfo fileInfo) => File.ReadAllBytes(fileInfo.FullName);
    }
}