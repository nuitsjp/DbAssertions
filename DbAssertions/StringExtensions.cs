using System;

namespace DbAssertions;

/// <summary>
/// String extension methods class.
/// </summary>
internal static class StringExtensions
{
    /// <summary>
    /// Get schema name.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    internal static string GetSchemaName(this string fileName) =>
        fileName.Substring(1, fileName.IndexOf("]", StringComparison.Ordinal) - 1);

    /// <summary>
    /// Get table name.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    internal static string GetTableName(this string fileName)
    {
        var schemaName = fileName.GetSchemaName();

        var tableName = fileName.Substring(schemaName.Length + 4);
        tableName = tableName.Substring(0, tableName.IndexOf("]", StringComparison.Ordinal));
        return tableName;
    }
}