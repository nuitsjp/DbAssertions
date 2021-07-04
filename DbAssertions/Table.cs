using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DbAssertions
{
    /// <summary>
    /// テーブルを表すクラス
    /// </summary>
    public class Table
    {
        /// <summary>
        /// インスタンスを生成する
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
        /// スキーマ名を取得する
        /// </summary>
        public string SchemaName { get; }

        /// <summary>
        /// テーブル名を取得する
        /// </summary>
        public string TableName { get; }

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
        /// 文字列表現を取得する
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"[{SchemaName}].[{TableName}]";

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

        private class DatabaseRow : IRow
        {
            private readonly IDataReader _dataReader;

            public DatabaseRow(IDataReader dataReader)
            {
                _dataReader = dataReader;
            }

            public object this[Column column] => _dataReader[column.ColumnName];
        }
    }
}