using System;
using System.Net;

namespace DbAssertions
{
    public class HostNameColumnOperator : IColumnOperator
    {
        public static readonly string DefaultLabel = "HostName";

        internal static IHostNameProvider HostNameProvider { get; set; } = new HostNameProvider();

        public string ToExpected(Column column, int rowNumber, string firstCell, string secondCell)
        {
            if (Equals(firstCell, secondCell))
            {
                if (Equals(firstCell, HostNameProvider.GetHostName()))
                {
                    return DefaultLabel;
                }

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

            if (Equals(actualCell, HostNameProvider.GetHostName()))
            {
                return true;
            }

            return false;
        }
    }
}