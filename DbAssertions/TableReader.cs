using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;

namespace DbAssertions
{
    /// <summary>
    /// Reader for table rows.
    /// </summary>
    internal class TableReader : ITableReader
    {
        /// <summary>
        /// Columns.
        /// </summary>
        private readonly IList<Column> _columns;

        /// <summary>
        /// Create instance.
        /// </summary>
        /// <param name="columns"></param>
        public TableReader(IList<Column> columns)
        {
            _columns = columns;
        }

        /// <summary>
        /// Read all lines.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public IRow[] ReadAllRows(FileInfo fileInfo)
        {
            using var streamReader = new StreamReader(fileInfo.OpenRead(), Encoding.UTF8);
            using var firstCsv = new CsvReader(streamReader, CultureInfo.InvariantCulture);
            return ReadAllRows(firstCsv).ToArray();
        }

        /// <summary>
        /// Read all lines.
        /// </summary>
        /// <param name="csvReader"></param>
        /// <returns></returns>
        private IEnumerable<IRow> ReadAllRows(CsvReader csvReader)
        {
            while (csvReader.Read())
            {
                yield return new CsvRow(ReadCells(csvReader), _columns);
            }
        }

        /// <summary>
        /// Read cells of row.
        /// </summary>
        /// <param name="csvReader"></param>
        /// <returns></returns>
        private IEnumerable<string> ReadCells(CsvReader csvReader)
        {
            for (int i = 0; i < _columns.Count; i++)
            {
                yield return csvReader.GetField(i);
            }
        }

        /// <summary>
        /// Row of the CSV file.
        /// </summary>
        private class CsvRow : IRow
        {
            /// <summary>
            /// Cells.
            /// </summary>
            private readonly string[] _cells;

            /// <summary>
            /// Columns.
            /// </summary>
            private readonly IList<Column> _columns;

            /// <summary>
            /// Create instance.
            /// </summary>
            /// <param name="cells"></param>
            /// <param name="columns"></param>
            public CsvRow(IEnumerable<string> cells, IList<Column> columns)
            {
                _columns = columns;
                _cells = cells.ToArray();
            }

            /// <summary>
            /// Get the cell in the specified column.
            /// </summary>
            /// <param name="column"></param>
            /// <returns></returns>
            public object this[Column column] => _cells[_columns.IndexOf(column)];
        }

    }
}