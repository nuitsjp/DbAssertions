using System;
using System.Net;

namespace DbAssertions
{
    /// <summary>
    /// Host Name IColumnOperator.
    /// </summary>
    public class HostNameColumnOperator : IColumnOperator
    {
        /// <summary>
        /// Default value of the label to be written in the expected file.
        /// </summary>
        public const string DefaultLabel = "HostName";

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
                if (Equals(firstCell, Dns.GetHostName()))
                {
                    // If both the first and second are equal to the hostname.
                    // Return default label.
                    return DefaultLabel;
                }

                // If the hostname does not match, the first time is assumed to be the expected.
                return firstCell;
            }

            throw DbAssertionsException.FromUnableToExpected(column, rowNumber, firstCell, secondCell);
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

            if (Equals(actualCell, Dns.GetHostName()))
            {
                return true;
            }

            return false;
        }
    }
}