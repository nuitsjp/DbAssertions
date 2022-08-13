using System;
using System.IO;

namespace DbAssertions
{
    /// <summary>
    /// FileInfo拡張メソッドクラス
    /// </summary>
    internal static class FileInfoExtensions
    {
        internal static string GetSchemaName(this FileInfo fileInfo) => fileInfo.Name.GetSchemaName();

        internal static string GetTableName(this FileInfo fileInfo) => fileInfo.Name.GetTableName();
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