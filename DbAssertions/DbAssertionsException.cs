using System;

namespace DbAssertions
{
    /// <summary>
    /// Exception to assertion error.
    /// </summary>
    public class DbAssertionsException : Exception
    {
        /// <summary>
        /// Create instance.
        /// </summary>
        /// <param name="message"></param>
        public DbAssertionsException(string message) : base(message){}

        public static DbAssertionsException FromUnableToExpected(Column column, int rowNumber, string firstCell, string secondCell)
        {
            return new($"[{column.DatabaseName}].[{column.SchemaName}].[{column.TableName}] テーブル {rowNumber} 行目の [{column.ColumnName}] 列の1回目 [{firstCell}] と2回目 [{secondCell}] が不一致です。");
        }
    }
}