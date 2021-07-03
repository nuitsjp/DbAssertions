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

        /// <summary>
        /// データベース名
        /// </summary>
        private readonly string _databaseName;

        /// <summary>
        /// スキーマ名
        /// </summary>
        private readonly string _schemaName;

        /// <summary>
        /// テーブル名
        /// </summary>
        private readonly string _tableName;

        /// <summary>
        /// カラム名
        /// </summary>
        private readonly string _columnName;

        /// <summary>
        /// カラム型
        /// </summary>
        private readonly ColumnType _columnType;

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
        public Column(string databaseName, string schemaName, string tableName, string columnName, ColumnType columnType, bool isPrimaryKey, int primaryKeyOrdinal)
        {
            _tableName = tableName;
            _columnName = columnName;
            _columnType = columnType;
            IsPrimaryKey = isPrimaryKey;
            PrimaryKeyOrdinal = primaryKeyOrdinal;
            _databaseName = databaseName;
            _schemaName = schemaName;
        }

        public string SchemaName => _schemaName;
        public string TableName => _tableName;
        public string ColumnName => _columnName;

        public ColumnType ColumnType => _columnType;

        public bool IsPrimaryKey { get; }

        public int PrimaryKeyOrdinal { get; }

        /// <summary>
        /// 値を比較し、期待結果ファイルに設定すべき値に変換する
        /// </summary>
        /// <param name="firstCell"></param>
        /// <param name="secondCell"></param>
        /// <param name="rowNumber"></param>
        /// <returns></returns>
        internal string ToExpected(string firstCell, string secondCell, int rowNumber)
        {
            if (Equals(firstCell, secondCell))
            {
                // 値が等しければ、その値を採用する
                return firstCell;
            }

            // 値が異なり、日付カラムの場合
            if (_columnType == ColumnType.DateTime)
            {
                if(firstCell.Any() && secondCell.Any())
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
                    throw new DbAssertionsException($"[{_databaseName}].[{_schemaName}].[{_tableName}] テーブル {rowNumber} 行目の [{_columnName}] 列の1回目 [{firstDateTime}] が2回目 [{secondDateTime}] 以降の日付になっています。");
                }

                // いずれかの値が空の場合、現時点で想定していない値の為、エラーとする。
                throw new DbAssertionsException($"[{_databaseName}].[{_schemaName}].[{_tableName}] テーブル {rowNumber} 行目の [{_columnName}] 列の{(firstCell.Any() ? 2 : 1)}回目の値が空です。");
            }

            // 日付カラムではないのでエラー
            throw new DbAssertionsException($"[{_databaseName}].[{_schemaName}].[{_tableName}] テーブル {rowNumber} 行目の [{_columnName}] 列の1回目 [{firstCell}] と2回目 [{secondCell}] が不一致です。");
        }

        /// <summary>
        /// 値を比較する
        /// </summary>
        /// <param name="expectedCell"></param>
        /// <param name="actualCell"></param>
        /// <param name="lifeCycleColumns"></param>
        /// <param name="timeBeforeStart"></param>
        /// <returns></returns>
        internal bool Compare(string expectedCell, string actualCell, IEnumerable<LifeCycleColumn> lifeCycleColumns, DateTime timeBeforeStart)
        {
            if (Equals(expectedCell, actualCell))
            {
                // 値が一致
                return true;
            }

            var matchedLifeCycleColumns =
                lifeCycleColumns
                    .Where(x => x.Match(_databaseName, _schemaName, _tableName, _columnName))
                    .ToArray();
            if (matchedLifeCycleColumns.IsNullOrEmpty())
            {
                // ファイルサイクルカラムではない場合、値が違えば不一致
                return false;
            }

            if (expectedCell.IsNullOrEmpty() || actualCell.IsNullOrEmpty())
            {
                // いずれかだけが空の場合、不一致
                // 両方空の場合は、最初の評価でtrueが返されている
                return false;
            }

            if (matchedLifeCycleColumns.Any(x => x.LifeCycle == LifeCycle.Daily))
            {
                // 実行時ではなく、日次で更新される値は日付書式なら一致とする
                // これを増やしすぎるとテストにならないので要注意
                if (DateTime.TryParse(actualCell, out _))
                {
                    return true;
                }

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