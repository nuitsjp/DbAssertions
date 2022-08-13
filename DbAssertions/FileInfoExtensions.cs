using System.IO;

namespace DbAssertions
{
    /// <summary>
    /// FileInfo extended method class.
    /// </summary>
    internal static class FileInfoExtensions
    {
        /// <summary>
        /// Get schema name.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        internal static string GetSchemaName(this FileInfo fileInfo) => fileInfo.Name.GetSchemaName();

        /// <summary>
        /// Get table name.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        internal static string GetTableName(this FileInfo fileInfo) => fileInfo.Name.GetTableName();
    }
}