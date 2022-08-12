using System;

namespace DbAssertions
{
    public class RandomColumnOperator : IColumnOperator
    {
        public const string DefaultLabel = "Random";

        public string ToExpected(Column column, int rowNumber, string firstCell, string secondCell)
        {
            if (Equals(firstCell, secondCell))
            {
                return firstCell;
            }

            return DefaultLabel;
        }

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

            return false;

        }
    }
}