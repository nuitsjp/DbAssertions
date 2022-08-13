using System.IO;

namespace DbAssertions
{
    /// <summary>
    /// Reader for table rows.
    /// </summary>
    internal interface ITableReader
    {
        /// <summary>
        /// Read all lines.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        IRow[] ReadAllRows(FileInfo fileInfo);
    }
}