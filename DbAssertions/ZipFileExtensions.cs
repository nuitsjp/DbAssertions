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
    internal static class ZipFileExtensions
    {
        /// <summary>
        /// すべてのZipEntryを取得する
        /// </summary>
        /// <param name="zipFile"></param>
        /// <returns></returns>
        internal static IEnumerable<ZipEntry> GetZipEntries(this ZipFile zipFile)
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
        /// <param name="databaseName"></param>
        /// <returns></returns>
        internal static Table GetTable(this ZipEntry zipEntry, string databaseName) =>
            Table.Parse(
                zipEntry.Name.Substring(zipEntry.Name.IndexOf("/", StringComparison.Ordinal) + 1));
    }
}