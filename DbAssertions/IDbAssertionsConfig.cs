namespace DbAssertions
{
    /// <summary>
    /// Settings for using DbAssertions.
    /// </summary>
    public interface IDbAssertionsConfig
    {
        /// <summary>
        /// Add IColumnOperator with applicable conditions.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="columnType"></param>
        /// <param name="columnOperator"></param>
        void AddColumnOperator(
            string? databaseName,
            string? schemaName,
            string? tableName,
            string? columnName,
            ColumnType? columnType,
            IColumnOperator columnOperator);

        /// <summary>
        /// Get IColumnOperator that matches the specified condition column.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="columnType"></param>
        /// <returns></returns>
        IColumnOperator GetColumnOperator(
            string databaseName,
            string schemaName,
            string tableName,
            string columnName,
            ColumnType columnType);
    }
}