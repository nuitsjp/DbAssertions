using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        public static IDbAssertionsConfig Deserialize(string filePath)
        {
            IDbAssertionsConfig config = new DbAssertionsConfig();
            dynamic json = JsonConvert.DeserializeObject(File.ReadAllText(filePath, Encoding.UTF8))!;
            var columnOperatorConditions = (JArray)json.ColumnOperatorConditions!;
            foreach (dynamic columnOperatorCondition in columnOperatorConditions)
            {
                config.AddColumnOperator(
                    (string)columnOperatorCondition.DatabaseName,
                    (string)columnOperatorCondition.SchemaName,
                    (string)columnOperatorCondition.TableName,
                    (string)columnOperatorCondition.ColumnName,
                    GetColumnType((string)columnOperatorCondition.ColumnType),
                    (string)columnOperatorCondition.ColumnOperator switch
                    {
                        "HostName" => ColumnOperators.HostName,
                        "RunTime" => ColumnOperators.RunTime,
                        "Random" => ColumnOperators.Random,
                        "SetupTime" => ColumnOperators.SetupTime,
                        _ => throw new DbAssertionsException($"ColumnOperator {columnOperatorCondition.ColumnOperator} does not exist.")
                    });
            }
            return config;
        }

        private static ColumnType? GetColumnType(string? value)
        {
            if (value.IsNullOrEmpty())
            {
                return null;
            }

            if (Enum.TryParse(value, out ColumnType columnType))
            {
                return columnType;
            }

            throw new DbAssertionsException($"ColumnType {value} does not exist.");
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