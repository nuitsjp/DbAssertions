using System;

namespace DbAssertions
{
    /// <summary>
    /// 実行時などに値が更新される可能性のあるカラムを表すクラス
    /// </summary>
    public class SpecificColumn
    {
        /// <summary>
        /// データベース名
        /// </summary>
        private readonly string? _databaseName;
        /// <summary>
        /// スキーマ名
        /// </summary>
        private readonly string? _schemaName;
        /// <summary>
        /// テーブル名
        /// </summary>
        private readonly string? _tableName;
        /// <summary>
        /// カラム名
        /// </summary>
        private readonly string _columnName;

        /// <summary>
        /// インスタンスを生成する
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="lifeCycle"></param>
        public SpecificColumn(string? databaseName, string? schemaName, string? tableName, string columnName, LifeCycle lifeCycle)
        {
            _databaseName = databaseName;
            _schemaName = schemaName;
            _tableName = tableName;
            _columnName = columnName;
            LifeCycle = lifeCycle;
        }

        /// <summary>
        /// ライフサイクルを取得する
        /// </summary>
        public LifeCycle LifeCycle { get; }

        /// <summary>
        /// 対象のカラムが該当するかどうか判定する
        /// </summary>
        /// <param name="database"></param>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public bool Match(string database, string schema, string table, string column)
        {
            // nullの値は一致するものとして判断する
            return
                (_databaseName is null || Equals(_databaseName, database))
                && (_schemaName is null || Equals(_schemaName, schema))
                && (_tableName is null || Equals(_tableName, table))
                && Equals(_columnName, column);
        }
    }
}