using System;

namespace DbAssertions
{
    public class RandomColumnOperator : IColumnOperator
    {
        /// <summary>
        /// Default value of the label to be written in the expected file.
        /// </summary>
        public const string DefaultLabel = "Random";

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
                return firstCell;
            }

            return DefaultLabel;
        }

        /// <summary>
        /// Compare actual with expected.
        /// </summary>
        /// <param name="expectedCell"></param>
        /// <param name="actualCell"></param>
        /// <param name="timeBeforeStart"></param>
        /// <returns></returns>
        public bool Compare(string expectedCell, string actualCell, DateTime timeBeforeStart)
        {
            if (Equals(actualCell, expectedCell))
            {
                return true;
            }

            if (Equals(expectedCell, DefaultLabel))
            {
                return true;
            }

            // For ignore, returns true even if actual and expect are different and no DefaultLabel is set for the expected value.
            // In the case of random, false if DefaultLabel is not set.
            return false;

        }
    }
}