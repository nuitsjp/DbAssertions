using System;

namespace DbAssertions
{
    public class IgnoreColumnOperator : IColumnOperator
    {
        public static readonly string DefaultLabel = "Ignore";

        public string ToExpected(Column column, int rowNumber, string firstCell, string secondCell)
            => DefaultLabel;

        public bool Compare(string expectedCell, string actualCell, DateTime timeBeforeStart)
            => true;
    }
}