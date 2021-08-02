using System;
using System.Net;

namespace DbAssertions
{
    public class HostNameColumnType : ISpecificColumnType
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

        public string ToExpected(string firstCell, string secondCell)
        {
            if (Equals(firstCell, secondCell))
            {
                return firstCell;
            }
            return HostNameLabel;
        }
    }
}