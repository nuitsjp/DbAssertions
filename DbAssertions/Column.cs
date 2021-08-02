using System;
using System.Collections.Generic;
using System.Linq;

namespace DbAssertions
{
    /// <summary>
    /// テーブルカラムを表すクラス
    /// </summary>
    public class Column
    {
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
            //if (ColumnType == ColumnType.DateTime
            //    || ColumnType == ColumnType.DateTime2)
            //{
            //    _columnOperator = new RunTimeColumnOperator();
            //}
            //else
            //{
            //    _columnOperator = new DefaultColumnOperator();
            //}

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
        /// <returns></returns>
        internal string ToExpected(string firstCell, string secondCell, int rowNumber) => 
            _columnOperator.ToExpected(this, rowNumber, firstCell, secondCell);

        /// <summary>
        /// 値を比較する
        /// </summary>
        /// <param name="expectedCell"></param>
        /// <param name="actualCell"></param>
        /// <param name="specificColumns"></param>
        /// <param name="timeBeforeStart"></param>
        /// <returns></returns>
        internal bool Compare(string expectedCell, string actualCell, IEnumerable<SpecificColumn> specificColumns, DateTime timeBeforeStart)
            => _columnOperator.Compare(expectedCell, actualCell, timeBeforeStart);
    }

    public class DefaultColumnOperator : IColumnOperator
    {
        /// <summary>
        /// 実行ごとに値が変わるセルを表す文字列
        /// </summary>
        internal const string TimeAfterStart = "TimeAfterStart";

        public string ToExpected(Column column, int rowNumber, string firstCell, string secondCell)
        {
            if (Equals(firstCell, secondCell))
            {
                // 値が等しければ、その値を採用する
                return firstCell;
            }

            // 値が異なり、日付カラムの場合
            if (column.ColumnType == ColumnType.DateTime
                || column.ColumnType == ColumnType.DateTime2)
            {
                if (firstCell.Any() && secondCell.Any())
                {
                    // いずれの値も空ではなかった場合、日付に変換する
                    var firstDateTime = DateTime.Parse(firstCell);
                    var secondDateTime = DateTime.Parse(secondCell);
                    if (firstDateTime <= secondDateTime)
                    {
                        // 2回目の値が新しければ、それは実行の都度更新される値と判断し、実行後の時刻になる値として
                        // そのラベルを採用する
                        return TimeAfterStart;
                    }

                    // 1つ目の日付が新しい場合、想定していない値の為、エラーとする
                    throw DbAssertionsException.FromFirstCellIsNewerThanSecondCell(column, rowNumber, firstCell, secondCell);
                }

                // いずれかの値が空の場合、現時点で想定していない値の為、エラーとする。
                throw DbAssertionsException.FromOneOfThemIsEmpty(column, rowNumber, firstCell, secondCell);
            }

            // 日付カラムではないのでエラー
            throw DbAssertionsException.FromUnableToExpected(column, rowNumber, firstCell, secondCell);
        }

        public bool Compare(string expectedCell, string actualCell, DateTime timeBeforeStart)
        {
            if (Equals(expectedCell, actualCell))
            {
                // 値が一致
                return true;
            }

            if (expectedCell.IsNullOrEmpty() || actualCell.IsNullOrEmpty())
            {
                // いずれかだけが空の場合、不一致
                // 両方空の場合は、最初の評価でtrueが返されている
                return false;
            }

            if (Equals(expectedCell, TimeAfterStart))
            {
                // 期待値がTimeAfterStartならテスト開始前の時刻より、actualが新しければ一致
                return timeBeforeStart <= DateTime.Parse(actualCell);
            }

            // それ以外は不一致
            return false;
        }

    }

    public class RunTimeColumnOperator : IColumnOperator
    {
        /// <summary>
        /// 実行ごとに値が変わるセルを表す文字列
        /// </summary>
        internal const string TimeAfterStart = "TimeAfterStart";

        public string ToExpected(Column column, int rowNumber, string firstCell, string secondCell)
        {
            if (Equals(firstCell, secondCell))
            {
                // 値が等しければ、その値を採用する
                return firstCell;
            }

            if (firstCell.Any() && secondCell.Any())
            {
                // いずれの値も空ではなかった場合、日付に変換する
                var firstDateTime = DateTime.Parse(firstCell);
                var secondDateTime = DateTime.Parse(secondCell);
                if (firstDateTime <= secondDateTime)
                {
                    // 2回目の値が新しければ、それは実行の都度更新される値と判断し、実行後の時刻になる値として
                    // そのラベルを採用する
                    return TimeAfterStart;
                }

                // 1つ目の日付が新しい場合、想定していない値の為、エラーとする
                throw DbAssertionsException.FromFirstCellIsNewerThanSecondCell(column, rowNumber, firstCell, secondCell);
            }

            // いずれかの値が空の場合、現時点で想定していない値の為、エラーとする。
            throw DbAssertionsException.FromOneOfThemIsEmpty(column, rowNumber, firstCell, secondCell);
        }

        public bool Compare(string expectedCell, string actualCell, DateTime timeBeforeStart)
        {
            if (Equals(expectedCell, actualCell))
            {
                // 値が一致
                return true;
            }

            if (expectedCell.IsNullOrEmpty() || actualCell.IsNullOrEmpty())
            {
                // いずれかだけが空の場合、不一致
                // 両方空の場合は、最初の評価でtrueが返されている
                return false;
            }

            if (Equals(expectedCell, TimeAfterStart))
            {
                // 期待値がTimeAfterStartならテスト開始前の時刻より、actualが新しければ一致
                return timeBeforeStart <= DateTime.Parse(actualCell);
            }

            // それ以外は不一致
            return false;
        }
    }
}