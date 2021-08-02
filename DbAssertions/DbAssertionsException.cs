using System;

namespace DbAssertions
{
    /// <summary>
    /// アサーションエラーを表す例外
    /// </summary>
    public class DbAssertionsException : Exception
    {
        /// <summary>
        /// インスタンスを生成する
        /// </summary>
        /// <param name="message"></param>
        public DbAssertionsException(string message) : base(message){}

        public static DbAssertionsException FromUnableToExpected(Column column, int rowNumber, string firstCell, string secondCell)
        {
            throw new DbAssertionsException($"[{column.DatabaseName}].[{column.SchemaName}].[{column.TableName}] テーブル {rowNumber} 行目の [{column.ColumnName}] 列の1回目 [{firstCell}] と2回目 [{secondCell}] が不一致です。");
        }
    }
}