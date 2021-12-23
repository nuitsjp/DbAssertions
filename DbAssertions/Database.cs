using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using ICSharpCode.SharpZipLib.Zip;

namespace DbAssertions
{
    /// <summary>
    /// テスト対象のデータベースを表す。
    /// </summary>
    public abstract class Database
    {
        /// <summary>
        /// データベース
        /// </summary>
        public string DatabaseName { get; }

        /// <summary>
        /// インスタンスを初期化する
        /// </summary>
        /// <param name="databaseName"></param>
        protected Database(string databaseName)
        {
            DatabaseName = databaseName;
        }

        public abstract IDbConnection OpenConnection();

        /// <summary>
        /// 1回目のエクスポートを実行する
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
        /// ２回目のエクスポートを実行し、期待結果ファイルを作成する
        /// </summary>
        /// <param name="directoryInfo"></param>
        /// <param name="initializeDateTime"></param>
        public void SecondExport(
            DirectoryInfo directoryInfo,
            DateTime initializeDateTime) => SecondExport(directoryInfo, initializeDateTime, new DbAssertionsConfig());

        /// <summary>
        /// ２回目のエクスポートを実行し、期待結果ファイルを作成する
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
                throw new InvalidOperationException("初回エクスポートフォルダが存在しません");
            }

            var tables = GetTables(config);

            // 2回目のエクスポートディレクトリを作成する
            var secondDirectoryInfo = directoryInfo.GetDirectory("Second");
            secondDirectoryInfo.ReCreate();
            // 期待結果保管ディレクトリを作成する
            var expectedDirectoryInfo = directoryInfo.GetDirectory("Expected");
            expectedDirectoryInfo.ReCreate();

            // １回目のエクスポートファイルから、対象テーブルを取得する。
            // この時、対象のDBのファイルだけを取得する
            var tableFiles = 
                firstDirectoryInfo
                    .GetFiles("*.csv");

            // 並列処理で全テーブル分の処理を実施する
            Parallel.ForEach(tableFiles, firstTableFile =>
            {
                // １回目のファイル名からテーブルオブジェクトを作成する
                var schemaName = firstTableFile.Name.GetSchemaName();
                var tableName = firstTableFile.Name.GetTableName();
                var table = tables.Single(x => x.SchemaName == schemaName && x.TableName == tableName);

                // ２回目のエクスポートを実行する
                var secondTableFile = Export(table, secondDirectoryInfo);

                // テキストが不一致の場合、実行ごとに変化のあるセルに対応しつつ期待結果ファイルを作成する
                using var expectedCsv = new CsvWriter(
                    new StreamWriter(File.Open(expectedDirectoryInfo.GetFile(firstTableFile.Name).FullName, FileMode.Create)),
                    CultureInfo.InvariantCulture);

                // 1回目と2回目の全CSV行を読み込む
                ITableReader tableReader = new TableReader(table.Columns);
                var firstRecords = tableReader.ReadAllRows(firstTableFile);
                var secondRecords = tableReader.ReadAllRows(secondTableFile);

                if (firstRecords.Length != secondRecords.Length)
                {
                    // 実行ごとに行数が変わるようなケースは対応しない。
                    // その場合、対象テーブルを除外して個別にテストすること。
                    throw new DbAssertionsException($@"ファイル {firstTableFile.Name} の行数が一致しませんでした。");
                }

                // 行ごとに処理を実施する
                for (var rowNumber = 0; rowNumber < firstRecords.Length; rowNumber++)
                {
                    // 全列をオブジェクト化する
                    var firstRecord = firstRecords[rowNumber];
                    var secondRecord = secondRecords[rowNumber];

                    // 列ごとに処理する
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

            // 期待結果をzipファイルに圧縮する
            FastZip fastZip = new();
            fastZip.CreateZip(
                directoryInfo.GetFile($"Expected{DatabaseName}.zip").FullName,
                expectedDirectoryInfo.FullName,
                false,
                null);

        }

        /// <summary>
        /// データベースの値を比較する
        /// </summary>
        /// <param name="expectedFileInfo"></param>
        /// <param name="setupCompletionTime"></param>
        /// <param name="config"></param>
        /// <param name="directoryInfo"></param>
        /// <param name="because"></param>
        /// <param name="becauseArgs"></param>
        public CompareResult Compare(
            FileInfo expectedFileInfo,
            DateTime setupCompletionTime,
            IDbAssertionsConfig config,
            DirectoryInfo directoryInfo,
            string because = "",
            params object[] becauseArgs)
        {
            var tables = GetTables(config);

            // 期待結果zipファイルを展開する
            using var compressStreamA = expectedFileInfo.OpenRead();
            using var zipFile = new ZipFile(compressStreamA);

            // データをExportしたときに文字列化する。その際の精度の問題で開始時刻より、実施時刻が前になることがある
            // 一旦この形で対応する
            var timeBeforeStart = DateTime.Parse(setupCompletionTime.ToString(CultureInfo.InvariantCulture));

            CompareResult compareResult = new();
            // zipファイルから対象データベースのテーブルファイルを取得し、並列処理する
            Parallel.ForEach(zipFile.GetZipEntries(), zipEntry =>
            {
                var schemaName = zipEntry.GetSchemaName();
                var tableName = zipEntry.GetTableName();
                var table = tables.SingleOrDefault(x => x.SchemaName == schemaName && x.TableName == tableName);
                if (table == null)
                {
                    compareResult.AddMismatchedMessage($@"期待値の設定されているテーブル [{schemaName}].[{tableName}] がデータベースに存在しませんでした。");
                    return;
                }
                var actualTableFile = Export(table, directoryInfo);
                
                var tableReader = new TableReader(table.Columns);
                // ReSharper disable once AccessToDisposedClosure
                var expectedRecords = tableReader.ReadAllRows(new StreamReader(zipFile.GetInputStream(zipEntry), Encoding.UTF8));
                var actualRecords = tableReader.ReadAllRows(actualTableFile);
                if (expectedRecords.Length != actualRecords.Length)
                {
                    // 実行ごとに行数が変わるようなケースは対応しない。
                    // その場合、対象テーブルを除外して個別にテストすること。
                    compareResult.AddMismatchedMessage($@"テーブル {table} の行数が期待値と一致しませんでした。期待値 {expectedRecords.Length} 行、実際値 {actualRecords.Length} 行。");
                    return;
                }


                // 行ごとの処理を実行する
                for (var rowNumber = 0; rowNumber < expectedRecords.Length; rowNumber++)
                {
                    // 全列をオブジェクト化する
                    var expectedRecordRecord = expectedRecords[rowNumber];
                    var actualRecord = actualRecords[rowNumber];

                    // 列ごとに処理する
                    foreach (var column in table.Columns)
                    {
                        var expectedRecordCell = (string)expectedRecordRecord[column];
                        var actualRecordCell = (string)actualRecord[column];
                        if (!column.Compare(expectedRecordCell, actualRecordCell, timeBeforeStart))
                        {
                            compareResult.AddMismatchedMessage(expectedRecordCell == Column.TimeAfterStart
                                ? $@"{table} テーブル {rowNumber + 1} 行目の {column.ColumnName} 列が一致しませんでした。DB初期化完了時刻 {timeBeforeStart} 、実際値 {actualRecordCell}。"
                                : $@"{table} テーブル {rowNumber + 1} 行目の {column.ColumnName} 列が一致しませんでした。期待値 {expectedRecordCell} 、実際値 {actualRecordCell}。");
                        }
                    }
                }
            });

            return compareResult;
        }

        /// <summary>
        /// BCPコマンドを利用し、対象テーブルの値をCSVにエクスポートする。
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
        /// データベースのすべてのユーザーテーブルを取得する。
        /// </summary>
        /// <returns></returns>
        protected abstract List<Table> GetTables(IDbAssertionsConfig config);
    }
}