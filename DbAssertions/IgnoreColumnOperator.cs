using System;

namespace DbAssertions
{
    /// <summary>
    /// IColumnOperator that, for some reason, does not evaluate the column value and always assumes it is positive.
    /// Similar to Random. See <see cref="RandomColumnOperator"/> for details.
    /// </summary>
    public class IgnoreColumnOperator : IColumnOperator
    {
        /// <summary>
        /// Default value of the label to be written in the expected file.
        /// </summary>
        public const string DefaultLabel = "Ignore";

        /// <summary>
        /// Convert to expected.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="rowNumber"></param>
        /// <param name="firstCell"></param>
        /// <param name="secondCell"></param>
        /// <returns></returns>
        public string ToExpected(Column column, int rowNumber, string firstCell, string secondCell)
            => DefaultLabel;

        /// <summary>
        /// Compare actual with expected.
        /// </summary>
        /// <param name="expectedCell"></param>
        /// <param name="actualCell"></param>
        /// <param name="timeBeforeStart"></param>
        /// <returns></returns>
        public bool Compare(string expectedCell, string actualCell, DateTime timeBeforeStart)
            => true;
    }
}