using System;
using System.Linq;

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

        public static DbAssertionsException FromOneOfThemIsEmpty(Column column, int rowNumber, string firstCell, string secondCell)
        {
            return new ($"[{column.DatabaseName}].[{column.SchemaName}].[{column.TableName}] テーブル {rowNumber} 行目の [{column.ColumnName}] 列の{(firstCell.Any() ? 2 : 1)}回目の値が空です。");
        }

        public static DbAssertionsException FromFirstCellIsNewerThanSecondCell(Column column, int rowNumber, string firstCell, string secondCell)
        {
            return new($"[{column.DatabaseName}].[{column.SchemaName}].[{column.TableName}] テーブル {rowNumber} 行目の [{column.ColumnName}] 列の1回目 [{firstCell}] が2回目 [{secondCell}] 以降の日付になっています。");
        }

        public static DbAssertionsException FromUnableToExpected(Column column, int rowNumber, string firstCell, string secondCell)
        {
            return new($"[{column.DatabaseName}].[{column.SchemaName}].[{column.TableName}] テーブル {rowNumber} 行目の [{column.ColumnName}] 列の1回目 [{firstCell}] と2回目 [{secondCell}] が不一致です。");
        }
    }
}