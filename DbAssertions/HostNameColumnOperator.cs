using System;
using System.Net;

namespace DbAssertions
{
    public class HostNameColumnOperator : IColumnOperator
    {
        private static readonly string HostNameLabel = "HostName";

        public bool Compare(string expectedCell, string actualCell, DateTime timeBeforeStart)
        {
            if (Equals(expectedCell, actualCell))
            {
                return true;
            }

            if (Equals(expectedCell, HostNameLabel)
                && Equals(actualCell, Dns.GetHostName()))
            {
                return true;
            }

            return false;
        }

        public string ToExpected(Column column, int rowNumber, string firstCell, string secondCell)
        {
            if (Equals(firstCell, secondCell))
            {
                if (Equals(firstCell, Dns.GetHostName()))
                {
                    return HostNameLabel;
                }

                return firstCell;
            }

            throw DbAssertionsException.FromUnableToExpected(column, rowNumber, firstCell, secondCell);
        }
    }
}