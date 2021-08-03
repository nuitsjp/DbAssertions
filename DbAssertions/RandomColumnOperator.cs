using System;

namespace DbAssertions
{
    public class RandomColumnOperator : IColumnOperator
    {
        public static readonly string DefaultLabel = "Random";

        public string ToExpected(Column column, int rowNumber, string firstCell, string secondCell)
            => DefaultLabel;

        public bool Compare(string expectedCell, string actualCell, DateTime timeBeforeStart)
            => true;
    }
}