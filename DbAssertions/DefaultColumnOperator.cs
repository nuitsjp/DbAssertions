using System;
using System.Linq;

namespace DbAssertions
{
    public class DefaultColumnOperator : IColumnOperator
    {
        public string ToExpected(Column column, int rowNumber, string firstCell, string secondCell)
        {
            if (Equals(firstCell, secondCell))
            {
                // 値が等しければ、その値を採用する
                return firstCell;
            }

            throw DbAssertionsException.FromUnableToExpected(column, rowNumber, firstCell, secondCell);
        }

        public bool Compare(string expectedCell, string actualCell, DateTime timeBeforeStart)
        {
            if (Equals(expectedCell, actualCell))
            {
                // 値が一致
                return true;
            }

            // それ以外は不一致
            return false;
        }
    }
}