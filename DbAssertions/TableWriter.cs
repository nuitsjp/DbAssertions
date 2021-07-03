using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using CsvHelper;

namespace DbAssertions
{
    internal class TableWriter : ITableWriter
    {
        private readonly Table _table;
        private readonly List<Column> _columns;
        private readonly CsvWriter _csvWriter;

        public TableWriter(Table table, List<Column> columns, DirectoryInfo directoryInfo)
        {
            _table = table;
            _columns = columns;
            FileInfo = directoryInfo.GetFile($"{table}.csv");

            _csvWriter = new CsvWriter(new StreamWriter(FileInfo.Open(FileMode.Create)));
        }

        public void Dispose()
        {
            _csvWriter.Dispose();
        }

        public FileInfo FileInfo { get; }

        public void Write(IRow row)
        {
            foreach (var column in _columns)
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
                else
                {
                    _csvWriter.WriteField(value);
                }
            }
            _csvWriter.NextRecord();
        }
    }
}