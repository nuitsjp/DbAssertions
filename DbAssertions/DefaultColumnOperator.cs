using System;

namespace DbAssertions
{
    /// <summary>
    /// Default IColumnOperator.
    /// </summary>
    public class DefaultColumnOperator : IColumnOperator
    {
        /// <summary>
        /// Convert to expected.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="rowNumber"></param>
        /// <param name="firstCell"></param>
        /// <param name="secondCell"></param>
        /// <returns></returns>
        public string ToExpected(Column column, int rowNumber, string firstCell, string secondCell)
        {
            if (Equals(firstCell, secondCell))
            {
                // 値が等しければ、その値を採用する
                return firstCell;
            }

            throw DbAssertionsException.FromUnableToExpected(column, rowNumber, firstCell, secondCell);
        }

        /// <summary>
        /// Compare the actual with the expected.
        /// </summary>
        /// <param name="expectedCell"></param>
        /// <param name="actualCell"></param>
        /// <param name="timeBeforeStart"></param>
        /// <returns></returns>
        public bool Compare(string expectedCell, string actualCell, DateTime timeBeforeStart)
        {
            return Equals(expectedCell, actualCell);
        }
    }
}