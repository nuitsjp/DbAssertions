using System.IO;
using System.Text.RegularExpressions;

namespace DbAssertions.Test
{
    /// <summary>
    /// FileInfo拡張メソッドクラス
    /// </summary>
    public static class FileInfoExtensions
    {
        /// <summary>
        /// ファイル名を比較するための文字列を取得する。
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="ignoreNumbersInFileName">trueが指定された場合、ファイル名の数値は無視する</param>
        /// <returns></returns>
        internal static string GetComparisonName(this FileInfo fileInfo, bool ignoreNumbersInFileName = false) =>
            ignoreNumbersInFileName 
                ? Regex.Replace(fileInfo.Name, @"\d", string.Empty) 
                : fileInfo.Name;

        /// <summary>
        /// FileInfoAssertionsを生成する
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static FileInfoAssertions Should(this FileInfo instance)
        {
            return new (instance);
        }
    }
}