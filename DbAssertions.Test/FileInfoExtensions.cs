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

        public static void ReplaceContent(this FileInfo fileInfo, string oldValue, string newValue)
        {
            var content = File.ReadAllText(fileInfo.FullName, Utf8WithoutBom);
            File.WriteAllText(fileInfo.FullName, content.Replace(oldValue, newValue), Utf8WithoutBom);
        }
    }
}