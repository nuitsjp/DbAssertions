using System;
using System.Linq;

namespace DbAssertions
{
    /// <summary>
    /// Table column classes.
    /// </summary>
    public class Column
    {
        /// <summary>
        /// String representing the time before the start.
        /// </summary>
        internal const string TimeBeforeStart = "TimeBeforeStart";

        /// <summary>
        /// String representing a cell whose value changes with each execution.
        /// </summary>
        internal const string TimeAfterStart = "TimeAfterStart";

        private readonly IColumnOperator _columnOperator;
        private readonly IColumnOperatorProvider _columnOperatorProvider;

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="columnType"></param>
        /// <param name="isPrimaryKey"></param>
        /// <param name="primaryKeyOrdinal"></param>
        /// <param name="columnOperator"></param>
        /// <param name="columnOperatorProvider"></param>
        public Column(
            string databaseName, 
            string schemaName, 
            string tableName, 
            string columnName, 
            ColumnType columnType, 
            bool isPrimaryKey, 
            int primaryKeyOrdinal, 
            IColumnOperator columnOperator, 
            IColumnOperatorProvider columnOperatorProvider)
        {
            TableName = tableName;
            ColumnName = columnName;
            ColumnType = columnType;
            IsPrimaryKey = isPrimaryKey;
            PrimaryKeyOrdinal = primaryKeyOrdinal;
            _columnOperator = columnOperator;
            _columnOperatorProvider = columnOperatorProvider;
            DatabaseName = databaseName;
            SchemaName = schemaName;
        }

        /// <summary>
        /// Get database name
        /// </summary>
        public string DatabaseName { get; }

        /// <summary>
        /// Get schema name
        /// </summary>
        public string SchemaName { get; }

        /// <summary>
        /// Get table name
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// Get column name
        /// </summary>
        public string ColumnName { get; }

        /// <summary>
        /// Get column type
        /// </summary>
        public ColumnType ColumnType { get; }

        /// <summary>
        /// Obtain whether it is a primary key or not
        /// </summary>
        public bool IsPrimaryKey { get; }

        /// <summary>
        /// Get primary key order
        /// </summary>
        public int PrimaryKeyOrdinal { get; }

        /// <summary>
        /// The values are compared and converted to values that should be set in the expected result file.
        /// </summary>
        /// <param name="firstCell"></param>
        /// <param name="secondCell"></param>
        /// <param name="rowNumber"></param>
        /// <param name="initializedDateTime"></param>
        /// <returns></returns>
        internal string ToExpected(string firstCell, string secondCell, int rowNumber, DateTime initializedDateTime)
        {
            if (_columnOperator is IgnoreColumnOperator or HostNameColumnOperator)
            {
                // In the case of host names, the first and second values are the same.
                // However, at the time of testing, the values will be different when run on other hosts.
                // Therefore, set a string representing the host name.
                // In the case of Ignore, the first and second times may happen to coincide.
                // Therefore, always set a string representing Ignore.
                return _columnOperator.ToExpected(this, rowNumber, firstCell, secondCell);
            }

            if (Equals(firstCell, secondCell))
            {
                return firstCell;
            }

            if (firstCell.Any() && secondCell.Any())
            {
                // If both the first and second time have a value and are of different date types, then
                if (ColumnType == ColumnType.DateTime)
                {
                    // The value to be set is determined by comparing it with the time at the time of database initialization.
                    var secondDateTime = DateTime.Parse(secondCell);
                    if (secondDateTime <= initializedDateTime)
                    {
                        return TimeBeforeStart;
                    }

                    return TimeAfterStart;
                }
            }

            return _columnOperator.ToExpected(this, rowNumber, firstCell, secondCell);
        }

        /// <summary>
        /// Compare values.
        /// </summary>
        /// <param name="expectedCell"></param>
        /// <param name="actualCell"></param>
        /// <param name="timeBeforeStart"></param>
        /// <returns></returns>
        internal bool Compare(string expectedCell, string actualCell, DateTime timeBeforeStart)
        {
            // Obtain the target operator from the description of the expected value cell.
            if (_columnOperatorProvider.TryGetColumnOperator(expectedCell, out var columnOperator))
            {
                return columnOperator.Compare(expectedCell, actualCell, timeBeforeStart);
            }

            // If the operator could not be determined from the expected value cell, the values are first simply compared.
            if (Equals(expectedCell, actualCell))
            {
                return true;
            }


            if (expectedCell.Any() && expectedCell.Any())
            {
                // If both the first and second time have a value and are of different date types, then
                if (ColumnType == ColumnType.DateTime)
                {
                    // 日付オブジェクトに変換し、
                    var secondDateTime = DateTime.Parse(actualCell);
                    if (Equals(expectedCell, TimeBeforeStart)
                        && secondDateTime <= timeBeforeStart)
                    {
                        return true;
                    }
                    if (Equals(expectedCell, TimeAfterStart)
                        && timeBeforeStart <= secondDateTime)
                    {
                        return true;
                    }

                }
            }

            return _columnOperator.Compare(expectedCell, actualCell, timeBeforeStart);
        }
    }
}