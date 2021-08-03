using System;
using System.Collections.Generic;
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

        internal static string GetSchemaName(this ZipEntry zipEntry) => zipEntry.Name.GetSchemaName();

        internal static string GetTableName(this ZipEntry zipEntry) => zipEntry.Name.GetTableName();
    }

    internal static class StringExtensions
    {
        internal static string GetSchemaName(this string fileName) =>
            fileName.Substring(1, fileName.IndexOf("]", StringComparison.Ordinal) - 1);

        internal static string GetTableName(this string fileName)
        {
            var schemaName = fileName.GetSchemaName();

            var tableName = fileName.Substring(schemaName.Length + 4);
            tableName = tableName.Substring(0, tableName.IndexOf("]", StringComparison.Ordinal));
            return tableName;
        }
    }
}