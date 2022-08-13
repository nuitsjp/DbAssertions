using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using CsvHelper;

namespace DbAssertions
{
    /// <summary>
    /// Writer for table rows.
    /// </summary>
    internal class TableWriter : ITableWriter
    {
        /// <summary>
        /// Table.
        /// </summary>
        private readonly Table _table;

        /// <summary>
        /// Writer of the CSV file.
        /// </summary>
        private readonly CsvWriter _csvWriter;

        /// <summary>
        /// Create instance.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="directoryInfo"></param>
        public TableWriter(Table table, DirectoryInfo directoryInfo)
        {
            _table = table;
            FileInfo = directoryInfo.GetFile($"{table}.csv");

            _csvWriter = new CsvWriter(new StreamWriter(FileInfo.Open(FileMode.Create)), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Dispose resources.
        /// </summary>
        public void Dispose()
        {
            _csvWriter.Dispose();
        }

        /// <summary>
        /// CSV file.
        /// </summary>
        public FileInfo FileInfo { get; }

        /// <summary>
        /// Write one row.
        /// </summary>
        /// <param name="row"></param>
        public void Write(IRow row)
        {
            foreach (var column in _table.Columns)
            {
                var value = row[column];
                if (value == DBNull.Value)
                {
                    _csvWriter.WriteField(null);
                }
                else if (column.ColumnType == ColumnType.VarBinary)
                {
                    // Binary uses hashing to compress CSV files.
                    using var sha256 = SHA256.Create();
                    var hash = sha256.ComputeHash((byte[])value);
                    _csvWriter.WriteField(string.Concat(hash.Select(b => $"{b:x2}")));
                }
                else if (column.ColumnType == ColumnType.DateTime)
                {
                    // Date format is for backward compatibility when using BCL commands.
                    // Change the format to one that follows the culture when adding the ability to make the format variable.
                    _csvWriter.WriteField(((DateTime)value).ToString("yyyy/MM/dd H:mm:ss"));
                }
                else
                {
                    _csvWriter.WriteField(value);
                }
            }
            _csvWriter.NextRecord();
        }
    }
}