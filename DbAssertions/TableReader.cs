using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;

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
            using var firstCsv = new CsvReader(streamReader, CultureInfo.InvariantCulture);
            return ReadAllRows(firstCsv).ToArray();
        }

        private IEnumerable<IRow> ReadAllRows(CsvReader csvReader)
        {
            while (csvReader.Read())
            {
                yield return new CsvRow(ReadCells(csvReader), _columns);
            }
        }

        private IEnumerable<string> ReadCells(CsvReader csvReader)
        {
            for (int i = 0; i < _columns.Count; i++)
            {
                yield return csvReader.GetField(i);
            }
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

            public CsvRow(IEnumerable<string> cells, IList<Column> columns)
            {
                _columns = columns;
                _cells = cells.ToArray();
            }

            public object this[Column column] => _cells[_columns.IndexOf(column)];
        }

    }
}