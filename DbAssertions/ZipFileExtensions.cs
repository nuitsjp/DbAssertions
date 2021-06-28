using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;

namespace DbAssertions
{
    /// <summary>
    /// ZipFile拡張メソッドクラス
    /// </summary>
    public static class ZipFileExtensions
    {
        /// <summary>
        /// すべてのZipEntryを取得する
        /// </summary>
        /// <param name="zipFile"></param>
        /// <returns></returns>
        public static IEnumerable<ZipEntry> GetZipEntries(this ZipFile zipFile)
        {
            foreach (ZipEntry zipEntry in zipFile)
            {
                yield return zipEntry;
            }
        }

        /// <summary>
        /// ZipEntryに該当するテーブルを取得する
        /// </summary>
        /// <param name="zipEntry"></param>
        /// <returns></returns>
        public static Table GetTable(this ZipEntry zipEntry) =>
            Table.Parse(
                zipEntry.Name.Substring(zipEntry.Name.IndexOf("/", StringComparison.Ordinal) + 1));

        /// <summary>
        /// ZipEntryの名称比較を行うための名称を取得する
        /// </summary>
        /// <param name="zipEntry"></param>
        /// <param name="ignoreNumbersInFileName">trueの場合、数値は無視する</param>
        /// <returns></returns>
        public static string GetComparisonName(this ZipEntry zipEntry, bool ignoreNumbersInFileName = false) =>
            ignoreNumbersInFileName
                ? Regex.Replace(zipEntry.Name, @"\d", string.Empty)
                : zipEntry.Name;

        /// <summary>
        /// ZipEntryの内容をすべて読み取る
        /// </summary>
        /// <param name="zipFile"></param>
        /// <param name="zipEntry"></param>
        /// <returns></returns>
        public static byte[] ReadAllBytes(this ZipFile zipFile, ZipEntry zipEntry)
        {
            using var zipStream = zipFile.GetInputStream(zipEntry);
            using var memoryStream = new MemoryStream();

            zipStream.CopyTo(memoryStream);

            return memoryStream.ToArray();
        }
    }
}