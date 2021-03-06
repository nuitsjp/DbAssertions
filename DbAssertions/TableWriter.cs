using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using CsvHelper;

namespace DbAssertions
{
    internal class TableWriter : ITableWriter
    {
        private readonly Table _table;
        private readonly CsvWriter _csvWriter;

        public TableWriter(Table table, DirectoryInfo directoryInfo)
        {
            _table = table;
            FileInfo = directoryInfo.GetFile($"{table}.csv");

            _csvWriter = new CsvWriter(new StreamWriter(FileInfo.Open(FileMode.Create)), CultureInfo.InvariantCulture);
        }

        public void Dispose()
        {
            _csvWriter.Dispose();
        }

        public FileInfo FileInfo { get; }

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
                    using var sha256 = SHA256.Create();
                    var hash = sha256.ComputeHash((byte[])value);
                    _csvWriter.WriteField(string.Concat(hash.Select(b => $"{b:x2}")));
                }
                else if (column.ColumnType == ColumnType.DateTime)
                {
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