using System;
using System.IO;

namespace DbAssertions
{
    /// <summary>
    /// Writer for table rows.
    /// </summary>
    internal interface ITableWriter : IDisposable
    {
        /// <summary>
        /// Target file.
        /// </summary>
        FileInfo FileInfo { get; }

        /// <summary>
        /// Write one row.
        /// </summary>
        /// <param name="row"></param>
        void Write(IRow row);
    }
}