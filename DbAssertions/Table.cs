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
        /// <param name="databaseName"></param>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        public Table(string schemaName, string tableName)
        {
            SchemaName = schemaName;
            TableName = tableName;
        }

        /// <summary>
        /// スキーマ名を取得する
        /// </summary>
        public string SchemaName { get; }

        /// <summary>
        /// テーブル名を取得する
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// 文字列表現を取得する
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"[{SchemaName}].[{TableName}]";

        /// <summary>
        /// テーブルファイルからテーブルインスタンスを生成する
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Table Parse(string value)
        {
            var schemaName = value.Substring(1, value.IndexOf("]", StringComparison.Ordinal) - 1);

            var tableName = value.Substring(schemaName.Length + 4);
            tableName = tableName.Substring(0, tableName.IndexOf("]", StringComparison.Ordinal));
            return new Table(schemaName, tableName);
        }

        internal IEnumerable<IRow> ReadAllRows(IDbConnection connection, IList<Column> columns)
        {
            var primaryKeys =
                columns
                    .Where(x => x.IsPrimaryKey)
                    .OrderBy(x => x.PrimaryKeyOrdinal)
                    .Select(x => x.ColumnName)
                    .ToArray();

            var query = @$"
select
    {string.Join(", ", columns.Select(x => $"[{x.ColumnName}]"))}
from
    [{SchemaName}].[{TableName}]
{(primaryKeys.Any() ? "order by" : string.Empty)}
    {string.Join(", ", primaryKeys)}";

            using var command = connection.CreateCommand();
            command.CommandText = query;

            using var reader = command.ExecuteReader();

            var row = new Row(reader);

            while (reader.Read())
            {
                yield return row;
            }
        }

        private class Row : IRow
        {
            private readonly IDataReader _dataReader;

            public Row(IDataReader dataReader)
            {
                _dataReader = dataReader;
            }

            public object this[Column column] => _dataReader[column.ColumnName];
        }
    }
}