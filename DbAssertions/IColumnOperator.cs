using System;

namespace DbAssertions
{
    /// <summary>
    /// Operator for the column.
    /// </summary>
    public interface IColumnOperator
    {
        /// <summary>
        /// Convert to expected.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="rowNumber"></param>
        /// <param name="firstCell"></param>
        /// <param name="secondCell"></param>
        /// <returns></returns>
        string ToExpected(Column column, int rowNumber, string firstCell, string secondCell);

        /// <summary>
        /// Compare actual with expected.
        /// </summary>
        /// <param name="expectedCell"></param>
        /// <param name="actualCell"></param>
        /// <param name="timeBeforeStart"></param>
        /// <returns></returns>
        bool Compare(string expectedCell, string actualCell, DateTime timeBeforeStart);
    }
}