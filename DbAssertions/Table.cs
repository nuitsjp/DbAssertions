using System;

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
        public Table(string databaseName, string schemaName, string tableName)
        {
            DatabaseName = databaseName;
            SchemaName = schemaName;
            TableName = tableName;
        }

        /// <summary>
        /// データベース名を取得する
        /// </summary>
        public string DatabaseName { get; }
        
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
        public override string ToString() => $"[{DatabaseName}].[{SchemaName}].[{TableName}]";

        /// <summary>
        /// テーブルファイルからテーブルインスタンスを生成する
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Table Parse(string value)
        {
            var databaseName = value.Substring(1, value.IndexOf("]", StringComparison.Ordinal) - 1);
            
            var schemaName = value.Substring(databaseName.Length + 4);
            schemaName = schemaName.Substring(0, schemaName.IndexOf("]", StringComparison.Ordinal));
            var tableName = value.Substring(databaseName.Length + schemaName.Length + 7);
            tableName = tableName.Substring(0, tableName.IndexOf("]", StringComparison.Ordinal));
            return new Table(databaseName, schemaName, tableName);
        }
    }
}