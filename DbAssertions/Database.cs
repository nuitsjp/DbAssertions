using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;

namespace DbAssertions
{
    /// <summary>
    /// Database to be tested.
    /// </summary>
    public abstract class Database
    {
        /// <summary>
        /// Database name.
        /// </summary>
        public abstract string DatabaseName { get; }

        /// <summary>
        /// Database connection string.
        /// </summary>
        public abstract string ConnectionString { get; }

        public abstract IDbConnection OpenConnection();

        /// <summary>
        /// Export the first time.
        /// </summary>
        /// <param name="directoryInfo"></param>
        public void FirstExport(DirectoryInfo directoryInfo)
        {
            var exportDirectoryInfo = directoryInfo.GetDirectory("First").ReCreate();

            var tables = GetTables(new DbAssertionsConfig());

            Parallel.ForEach(tables, table =>
            {
                Export(table, exportDirectoryInfo);
            });
        }

        /// <summary>
        /// Export a second time and create an expected file.
        /// </summary>
        /// <param name="directoryInfo"></param>
        /// <param name="initializeDateTime"></param>
        public void SecondExport(
            DirectoryInfo directoryInfo,
            DateTime initializeDateTime) => SecondExport(directoryInfo, initializeDateTime, new DbAssertionsConfig());

        /// <summary>
        /// Export a second time and create an expected file.
        /// </summary>
        /// <param name="directoryInfo"></param>
        /// <param name="initializeDateTime"></param>
        /// <param name="config"></param>
        public void SecondExport(
            DirectoryInfo directoryInfo,
            DateTime initializeDateTime,
            IDbAssertionsConfig config)
        {
            var firstDirectoryInfo = directoryInfo.GetDirectory("First");
            if (firstDirectoryInfo.NotExist())
            {
                throw new InvalidOperationException("First export folder does not exist.");
            }

            var tables = GetTables(config);

            // Create a second export directory.
            var secondDirectoryInfo = directoryInfo.GetDirectory("Second");
            secondDirectoryInfo.ReCreate();
            // Create a directory for expected results.
            var expectedDirectoryInfo = directoryInfo.GetDirectory("Expected");
            expectedDirectoryInfo.ReCreate();

            // Obtain the target table from the first export file.
            // Retrieve only the target Database file.
            var tableFiles = 
                firstDirectoryInfo
                    .GetFiles("*.csv");

            // Parallel processing for all tables.
            Parallel.ForEach(tableFiles, firstTableFile =>
            {
                // Create a table object from the first file name.
                var schemaName = firstTableFile.Name.GetSchemaName();
                var tableName = firstTableFile.Name.GetTableName();
                var table = tables.Single(x => x.SchemaName == schemaName && x.TableName == tableName);

                // Export a second time.
                var secondTableFile = Export(table, secondDirectoryInfo);

                // If the text is mismatched, an expected file is created while corresponding to the cells that change with each run.
                using var expectedCsv = new CsvWriter(
                    new StreamWriter(File.Open(expectedDirectoryInfo.GetFile(firstTableFile.Name).FullName, FileMode.Create)),
                    CultureInfo.InvariantCulture);

                // Read all CSV rows for the first and second times.
                ITableReader tableReader = new TableReader(table.Columns);
                var firstRecords = tableReader.ReadAllRows(firstTableFile);
                var secondRecords = tableReader.ReadAllRows(secondTableFile);

                if (firstRecords.Length != secondRecords.Length)
                {
                    // Cases in which the number of rows changes with each execution are not supported.
                    // In such cases, the target table should be excluded and tested individually.
                    throw new DbAssertionsException($@"The number of rows in file {firstTableFile.Name} did not match.");
                }

                // Perform processing line by line
                for (var rowNumber = 0; rowNumber < firstRecords.Length; rowNumber++)
                {
                    // Objectify all columns
                    var firstRecord = firstRecords[rowNumber];
                    var secondRecord = secondRecords[rowNumber];

                    // Process by column.
                    foreach (var column in table.Columns)
                    {
                        var firstRecordCell = (string)firstRecord[column];
                        var secondRecordCell = (string)secondRecord[column];
                        var expectedRecordCell = column.ToExpected(firstRecordCell, secondRecordCell, rowNumber, initializeDateTime);
                        expectedCsv.WriteField(expectedRecordCell);
                    }
                    expectedCsv.NextRecord();
                }
            });
        }

        /// <summary>
        /// Compare database with expected.
        /// </summary>
        /// <param name="expectedFileInfo"></param>
        /// <param name="setupCompletionTime"></param>
        /// <param name="config"></param>
        /// <param name="directoryInfo"></param>
        /// <param name="because"></param>
        /// <param name="becauseArgs"></param>
        public CompareResult Compare(
            DirectoryInfo expectedFileInfo,
            DateTime setupCompletionTime,
            IDbAssertionsConfig config,
            DirectoryInfo directoryInfo,
            string because = "",
            params object[] becauseArgs)
        {
            var tables = GetTables(config);

            // String the data when it is Exported.
            // When truncation occurs due to accuracy issues in this process, the time of implementation may be earlier than the start time.
            // Therefore, the setup time is also truncated by converting it back to a string once.
            const string dateFormat = "yyyy/MM/dd HH:mm:ss";
            var timeBeforeStart = DateTime.ParseExact(setupCompletionTime.ToString(dateFormat, CultureInfo.InvariantCulture), dateFormat, null);

            CompareResult compareResult = new();
            // Obtain the table files of the target database from the expectation file and process them in parallel.
            Parallel.ForEach(expectedFileInfo.GetFiles(), tableFile =>
            {
                var schemaName = tableFile.GetSchemaName();
                var tableName = tableFile.GetTableName();
                var table = tables.SingleOrDefault(x => x.SchemaName == schemaName && x.TableName == tableName);
                if (table == null)
                {
                    compareResult.AddMismatchedMessage($"The table [{schemaName}].[{tableName}] for which the expectation is set did not exist in the database.");
                    return;
                }
                var actualTableFile = Export(table, directoryInfo);
                
                var tableReader = new TableReader(table.Columns);
                // ReSharper disable once AccessToDisposedClosure
                var expectedRecords = tableReader.ReadAllRows(tableFile);
                var actualRecords = tableReader.ReadAllRows(actualTableFile);
                if (expectedRecords.Length != actualRecords.Length)
                {
                    // Cases in which the number of rows changes with each execution are not supported.
                    // In such cases, the target table should be excluded and tested individually.
                    compareResult.AddMismatchedMessage($"The number of rows in table {table} did not match the expected value. Expected {expectedRecords.Length} rows, Actual {actualRecords.Length} rows.");
                    return;
                }


                // Perform line-by-line processing.
                for (var rowNumber = 0; rowNumber < expectedRecords.Length; rowNumber++)
                {
                    // Objectify all columns
                    var expectedRecordRecord = expectedRecords[rowNumber];
                    var actualRecord = actualRecords[rowNumber];

                    // Process by column.
                    foreach (var column in table.Columns)
                    {
                        var expectedRecordCell = (string)expectedRecordRecord[column];
                        var actualRecordCell = (string)actualRecord[column];
                        if (!column.Compare(expectedRecordCell, actualRecordCell, timeBeforeStart))
                        {
                            compareResult.AddMismatchedMessage(expectedRecordCell == Column.TimeAfterStart
                                ? $"Column {column.ColumnName} in row {rowNumber + 1} of table {table} did not match, DB initialization completion time [{timeBeforeStart}], actual value [{actualRecordCell}]."
                                : $"Column {column.ColumnName} in row {rowNumber + 1} of table {table} did not match. expected value [{expectedRecordCell}], actual value [{actualRecordCell}].");
                        }
                    }
                }
            });

            return compareResult;
        }

        /// <summary>
        /// Export the table to CSV.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="directoryInfo"></param>
        /// <returns></returns>
        public FileInfo Export(Table table, DirectoryInfo directoryInfo)
        {
            using var connection = OpenConnection();

            using ITableWriter tableWriter = new TableWriter(table, directoryInfo);

            try
            {
                foreach (var row in table.ReadAllRows(connection))
                {
                    tableWriter.Write(row);
                }

                return tableWriter.FileInfo;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Get all user tables.
        /// </summary>
        /// <returns></returns>
        protected abstract List<Table> GetTables(IDbAssertionsConfig config);
    }
}