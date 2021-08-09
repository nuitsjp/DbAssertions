using System;
using System.Linq;

namespace DbAssertions
{
    /// <summary>
    /// テーブルカラムを表すクラス
    /// </summary>
    public class Column
    {
        /// <summary>
        /// String representing the time before the start.
        /// </summary>
        internal const string TimeBeforeStart = "TimeBeforeStart";
        /// <summary>
        /// 実行ごとに値が変わるセルを表す文字列
        /// </summary>
        internal const string TimeAfterStart = "TimeAfterStart";

        private readonly IColumnOperator _columnOperator;

        /// <summary>
        /// インスタンスを生成する。
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="columnType"></param>
        /// <param name="isPrimaryKey"></param>
        /// <param name="primaryKeyOrdinal"></param>
        /// <param name="columnOperator"></param>
        public Column(string databaseName, string schemaName, string tableName, string columnName, ColumnType columnType, bool isPrimaryKey, int primaryKeyOrdinal, IColumnOperator columnOperator)
        {
            TableName = tableName;
            ColumnName = columnName;
            ColumnType = columnType;
            IsPrimaryKey = isPrimaryKey;
            PrimaryKeyOrdinal = primaryKeyOrdinal;
            _columnOperator = columnOperator;
            DatabaseName = databaseName;
            SchemaName = schemaName;
        }

        /// <summary>
        /// データベース名
        /// </summary>
        public string DatabaseName { get; }

        /// <summary>
        /// スキーマ名
        /// </summary>
        public string SchemaName { get; }

        /// <summary>
        /// テーブル名
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// カラム名
        /// </summary>
        public string ColumnName { get; }

        /// <summary>
        /// カラム型
        /// </summary>
        public ColumnType ColumnType { get; }

        public bool IsPrimaryKey { get; }

        public int PrimaryKeyOrdinal { get; }

        /// <summary>
        /// 値を比較し、期待結果ファイルに設定すべき値に変換する
        /// </summary>
        /// <param name="firstCell"></param>
        /// <param name="secondCell"></param>
        /// <param name="rowNumber"></param>
        /// <param name="initializedDateTime"></param>
        /// <returns></returns>
        internal string ToExpected(string firstCell, string secondCell, int rowNumber, DateTime initializedDateTime)
        {
            if (Equals(firstCell, secondCell))
            {
                return firstCell;
            }

            if (firstCell.Any() && secondCell.Any())
            {
                if (ColumnType == ColumnType.DateTime)
                {
                    // いずれの値も空ではなかった場合、日付に変換する
                    var secondDateTime = DateTime.Parse(secondCell);
                    if (secondDateTime <= initializedDateTime)
                    {
                        return TimeBeforeStart;
                    }

                    return TimeAfterStart;
                }
            }

            return _columnOperator.ToExpected(this, rowNumber, firstCell, secondCell);
        }

        /// <summary>
        /// 値を比較する
        /// </summary>
        /// <param name="expectedCell"></param>
        /// <param name="actualCell"></param>
        /// <param name="timeBeforeStart"></param>
        /// <returns></returns>
        internal bool Compare(string expectedCell, string actualCell, DateTime timeBeforeStart)
        {
            if (Equals(expectedCell, actualCell))
            {
                return true;
            }

            if (expectedCell.Any() && expectedCell.Any())
            {
                if (ColumnType == ColumnType.DateTime)
                {
                    var secondDateTime = DateTime.Parse(actualCell);
                    if (Equals(expectedCell, TimeBeforeStart)
                        && secondDateTime <= timeBeforeStart)
                    {
                        return true;
                    }
                    if (Equals(expectedCell, TimeAfterStart)
                        && timeBeforeStart <= secondDateTime)
                    {
                        return true;
                    }

                }
            }

            return _columnOperator.Compare(expectedCell, actualCell, timeBeforeStart);
        }
    }
}