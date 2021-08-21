using System;

namespace DbAssertions
{
    public class ColumnOperators
    {
        public static readonly IColumnOperator HostName = new HostNameColumnOperator();
        public static readonly IColumnOperator Random = new RandomColumnOperator();
        public static readonly IColumnOperator Ignore = new IgnoreColumnOperator();

        public static bool TryGetColumnOperator(string label, out IColumnOperator columnOperator)
        {
            if (Equals(label, HostNameColumnOperator.DefaultLabel))
            {
                columnOperator = Ignore;
                return true;
            }
            if (Equals(label, RandomColumnOperator.DefaultLabel))
            {
                columnOperator = Ignore;
                return true;
            }
            if (Equals(label, IgnoreColumnOperator.DefaultLabel))
            {
                columnOperator = Ignore;
                return true;
            }


            columnOperator = new InvalidLabelColumnOperator();
            return false;
        }

        private class InvalidLabelColumnOperator : IColumnOperator
        {
            public string ToExpected(Column column, int rowNumber, string firstCell, string secondCell)
            {
                throw new InvalidOperationException();
            }

            public bool Compare(string expectedCell, string actualCell, DateTime timeBeforeStart)
            {
                throw new InvalidOperationException();
            }
        }
    }
}