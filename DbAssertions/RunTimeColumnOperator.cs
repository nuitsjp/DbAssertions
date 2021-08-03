﻿using System;
using System.Linq;

namespace DbAssertions
{
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