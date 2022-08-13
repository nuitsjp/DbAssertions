using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DbAssertions
{
    /// <summary>
    /// Class representing a table
    /// </summary>
    public class Table
    {
        /// <summary>
        /// Create instance.
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        public Table(string schemaName, string tableName, List<Column> columns)
        {
            SchemaName = schemaName;
            TableName = tableName;
            Columns = columns;
        }

        /// <summary>
        /// Get schema name.
        /// </summary>
        public string SchemaName { get; }

        /// <summary>
        /// Get table name.
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// Get columns.
        /// </summary>
        public IList<Column> Columns { get; }

        private string[] PrimaryKeys
        {
            get
            {
                return Columns
                    .Where(x => x.IsPrimaryKey)
                    .OrderBy(x => x.PrimaryKeyOrdinal)
                    .Select(x => x.ColumnName)
                    .ToArray();
            }
        }

        /// <summary>
        /// Get a string representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"[{SchemaName}].[{TableName}]";

        /// <summary>
        /// Read all rows.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        internal IEnumerable<IRow> ReadAllRows(IDbConnection connection)
        {
            var primaryKeys = PrimaryKeys;

            var query = @$"
select
    {string.Join(", ", Columns.Select(x => $"[{x.ColumnName}]"))}
from
    [{SchemaName}].[{TableName}]
{(primaryKeys.Any() ? "order by" : string.Empty)}
    {string.Join(", ", primaryKeys)}";

            using var command = connection.CreateCommand();
            command.CommandText = query;

            using var reader = command.ExecuteReader();

            var row = new DatabaseRow(reader);

            while (reader.Read())
            {
                yield return row;
            }
        }

        /// <summary>
        /// Row of table.
        /// </summary>
        private class DatabaseRow : IRow
        {

            private readonly IDataReader _dataReader;

            /// <summary>
            /// Create instance.
            /// </summary>
            /// <param name="dataReader"></param>
            public DatabaseRow(IDataReader dataReader)
            {
                _dataReader = dataReader;
            }

            /// <summary>
            /// Get the cell in the specified column.
            /// </summary>
            /// <param name="column"></param>
            /// <returns></returns>
            public object this[Column column] => _dataReader[column.ColumnName];
        }
    }
}