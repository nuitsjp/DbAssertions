using System.IO;
using System.Text;

namespace DbAssertions.Test
{
    /// <summary>
    /// FileInfo拡張メソッドクラス
    /// </summary>
    public static class FileInfoExtensions
    {
        private static readonly Encoding Utf8WithoutBom = new UTF8Encoding(false);
        /// <summary>
        /// FileInfoAssertionsを生成する
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static FileInfoAssertions Should(this FileInfo instance)
        {
            return new (instance);
        }

        /// <summary>
        /// ファイルの内容を
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static string ReadAllText(this FileInfo instance)
            => File.ReadAllText(instance.FullName);
    }
}