using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace DbAssertions
{
    internal class TableReader : ITableReader
    {
        private readonly IList<Column> _columns;

        public TableReader(IList<Column> columns)
        {
            _columns = columns;
        }

        public IRow[] ReadAllRows(FileInfo fileInfo) => ReadAllRows(new StreamReader(fileInfo.OpenRead(), Encoding.UTF8));


        public IRow[] ReadAllRows(StreamReader streamReader)
        {
#if NET40
            using var firstCsv = new CsvReader(streamReader) { Configuration = { HasHeaderRecord = false } };
#else
            using var firstCsv = new CsvReader(streamReader, CultureInfo.InvariantCulture);
#endif

            return firstCsv
                .GetRecords<dynamic>()
                .Select(x => new CsvRow(x, _columns))
                .Cast<IRow>()
                .ToArray();
        }

        private class CsvRow : IRow
        {
            private readonly string[] _cells;

            private readonly IList<Column> _columns;

            public CsvRow(dynamic row, IList<Column> columns)
            {
                _columns = columns;
                _cells = ((IDictionary<string, object>)row).Values.Cast<string>().ToArray();
            }

            public object this[Column column] => _cells[_columns.IndexOf(column)];
        }

    }
}