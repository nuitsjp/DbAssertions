using System.Collections.Generic;
using System.Linq;

namespace DbAssertions
{
    public class DbAssertionsConfig : IDbAssertionsConfig
    {
        private readonly List<ColumnOperatorCondition> _conditions = new();

        public void AddColumnOperator(string? databaseName, string? schemaName, string? tableName, string? columnName,
            ColumnType? columnType, IColumnOperator columnOperator)
            => _conditions.Add(new ColumnOperatorCondition(databaseName, schemaName, tableName, columnName, columnType, columnOperator));

        public IColumnOperator DefaultColumnOperator { get; set; } = new DefaultColumnOperator();

        public IColumnOperator GetColumnOperator(string databaseName, string schemaName, string tableName, string columnName,
            ColumnType columnType)
        {
            var columnCondition = _conditions
                .FirstOrDefault(
                    x =>
                        (x.DatabaseName is null || databaseName.Contains(x.DatabaseName))
                        && (x.SchemaName is null || schemaName.Contains(x.SchemaName))
                        && (x.TableName is null || tableName.Contains(x.TableName))
                        && (x.ColumnName is null || columnName.Contains(x.ColumnName))
                        && (x.ColumnType is null || x.ColumnType == columnType));
            if (columnCondition is not null)
            {
                return columnCondition.ColumnOperator;
            }

            return DefaultColumnOperator;
        }

        private class ColumnOperatorCondition
        {
            public ColumnOperatorCondition(string? databaseName, string? schemaName, string? tableName, string? columnName, ColumnType? columnType, IColumnOperator columnOperator)
            {
                DatabaseName = databaseName;
                SchemaName = schemaName;
                TableName = tableName;
                ColumnName = columnName;
                ColumnType = columnType;
                ColumnOperator = columnOperator;
            }

            public string? DatabaseName { get; }
            public string? SchemaName { get; }
            public string? TableName { get; }
            public string? ColumnName { get; }
            public ColumnType? ColumnType { get; }
            public IColumnOperator ColumnOperator { get; }
        }

    }
}