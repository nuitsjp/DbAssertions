namespace DbAssertions
{
    public class ColumnOperators
    {
        public static readonly IColumnOperator HostName = new HostNameColumnOperator();
        public static readonly IColumnOperator Random = new RandomColumnOperator();
        public static readonly IColumnOperator RunTime = new RunTimeColumnOperator();
        public static readonly IColumnOperator SetupTime = new SetupTimeColumnOperator();
    }
}