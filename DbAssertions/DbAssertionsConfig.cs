using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DbAssertions
{
    /// <summary>
    /// Settings for using DbAssertions.
    /// </summary>
    public class DbAssertionsConfig : IDbAssertionsConfig
    {
        /// <summary>
        /// Condition to be applied to any table or column.
        /// </summary>
        private readonly List<ColumnOperatorCondition> _conditions = new();

        /// <summary>
        /// Add IColumnOperator with applicable conditions.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="columnType"></param>
        /// <param name="columnOperator"></param>
        public void AddColumnOperator(string? databaseName, string? schemaName, string? tableName, string? columnName,
            ColumnType? columnType, IColumnOperator columnOperator)
            => _conditions.Add(new ColumnOperatorCondition(databaseName, schemaName, tableName, columnName, columnType, columnOperator));

        /// <summary>
        /// Default IColumnOperator.
        /// </summary>
        public IColumnOperator DefaultColumnOperator { get; set; } = new DefaultColumnOperator();

        /// <summary>
        /// Get IColumnOperator that matches the specified condition column.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="columnType"></param>
        /// <returns></returns>
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
            
            return columnCondition?.ColumnOperator ?? DefaultColumnOperator;
        }

        /// <summary>
        /// Deserialize the config from the file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <exception cref="DbAssertionsException"></exception>
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
                        HostNameColumnOperator.DefaultLabel => new HostNameColumnOperator(),
                        RandomColumnOperator.DefaultLabel => new RandomColumnOperator(),
                        IgnoreColumnOperator.DefaultLabel => new IgnoreColumnOperator(),
                        _ => throw new DbAssertionsException($"ColumnOperator {columnOperatorCondition.ColumnOperator} does not exist.")
                    });
            }
            return config;
        }

        /// <summary>
        /// Get ColumnType from the string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="DbAssertionsException"></exception>
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

        /// <summary>
        /// Condition to apply IColumnOperator.
        /// </summary>
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