using System.IO;

namespace DbAssertions
{
    /// <summary>
    /// FileInfo拡張メソッドクラス
    /// </summary>
    public static class FileInfoExtensions
    {
        public static byte[] ReadAllBytes(this FileInfo fileInfo) => File.ReadAllBytes(fileInfo.FullName);

        public static bool NotExist(this FileInfo fileInfo) => !fileInfo.Exists;
    }
}