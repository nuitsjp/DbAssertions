using System;
using System.Net;

namespace DbAssertions
{
    public class SetupTimeColumnOperator : IColumnOperator
    {
        public string ToExpected(Column column, int rowNumber, string firstCell, string secondCell)
        {
            if (Equals(firstCell, secondCell))
            {
                return firstCell;
            }

            throw DbAssertionsException.FromUnableToExpected(column, rowNumber, firstCell, secondCell);
        }

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