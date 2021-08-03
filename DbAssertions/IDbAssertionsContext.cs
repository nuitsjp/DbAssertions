namespace DbAssertions
{
    public interface IDbAssertionsContext
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