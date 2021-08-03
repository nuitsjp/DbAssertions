namespace DbAssertions
{
    public interface IDbAssertionsConfig
    {
        void AddColumnOperator(
            string? databaseName,
            string? schemaName,
            string? tableName,
            string? columnName,
            ColumnType? columnType,
            IColumnOperator columnOperator);

        IColumnOperator GetColumnOperator(
            string databaseName,
            string schemaName,
            string tableName,
            string columnName,
            ColumnType columnType);
    }
}