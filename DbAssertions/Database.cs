using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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

        /// <summary>
        /// 接続文字列を取得する
        /// </summary>
        public abstract string ConnectionString { get; }

        public abstract IDbConnection OpenConnection();

        /// <summary>
        /// 1回目のエクスポートを実行する
        /// </summary>
        /// <param name="directoryInfo"></param>
        public void FirstExport(DirectoryInfo directoryInfo)
        {
            var exportDirectoryInfo = directoryInfo.GetDirectory("First").ReCreate();

            var tables = GetTables();

            Parallel.ForEach(tables, table =>
            {
                Export(table, exportDirectoryInfo);
            });
        }

        /// <summary>
        /// ２回目のエクスポートを実行し、期待結果ファイルを作成する
        /// </summary>
        /// <param name="directoryInfo"></param>
        public void SecondExport(
            DirectoryInfo directoryInfo)
        {
            var firstDirectoryInfo = directoryInfo.GetDirectory("First");
            if (firstDirectoryInfo.NotExist())
            {
                throw new InvalidOperationException("初回エクスポートフォルダが存在しません");
            }

            var tables = GetTables();

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
                using var expectedCsv = new CsvWriter(new StreamWriter(File.Open(expectedDirectoryInfo.GetFile(firstTableFile.Name).FullName, FileMode.Create)));

                // 1回目と2回目の全CSV行を読み込む
                var tableReader = new TableReader(table.Columns);

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

                    //if (firstRecordCells.Length != secondRecordCells.Length)
                    //{
                    //    // 列数が異なる場合、テーブル構造が変化しており実施手順に何らかの問題がある
                    //    // CsvHelperの仕様で1行目の列数が全行取得されるため、必ず1行目でエラーが発生する
                    //    // 2行目以降の列数エラーは、ここ以降の判定でセルの値の不一致として判断される
                    //    throw new DbAssertionsException($@"ファイル {firstTableFile.Name} の列数が一致しませんでした。");
                    //}

                    // 列ごとに処理する
                    foreach (var column in table.Columns)
                    {
                        var firstRecordCell = (string)firstRecord[column];
                        var secondRecordCell = (string)secondRecord[column];
                        var expectedRecordCell = column.ToExpected(firstRecordCell, secondRecordCell, rowNumber);
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
        /// <param name="lifeCycleColumns"></param>
        /// <param name="directoryInfo"></param>
        /// <param name="because"></param>
        /// <param name="becauseArgs"></param>
        public CompareResult Compare(
            FileInfo expectedFileInfo,
            DateTime setupCompletionTime,
            IEnumerable<LifeCycleColumn> lifeCycleColumns,
            DirectoryInfo directoryInfo,
            string because = "",
            params object[] becauseArgs)
        {
            var tables = GetTables();

            // 期待結果zipファイルを展開する
            using var compressStreamA = expectedFileInfo.OpenRead();
            using var zipFile = new ZipFile(compressStreamA);

            // データをExportしたときに文字列化する。その際の精度の問題で開始時刻より、実施時刻が前になることがある
            // 一旦この形で対応する
            var timeBeforeStart = DateTime.Parse(setupCompletionTime.ToString(CultureInfo.InvariantCulture));

            var lifeCycleColumnsArray = lifeCycleColumns as LifeCycleColumn[] ?? lifeCycleColumns.ToArray();

            CompareResult compareResult = new();
            // zipファイルから対象データベースのテーブルファイルを取得し、並列処理する
            Parallel.ForEach(zipFile.GetZipEntries(), zipEntry =>
            {
                var schemaName = zipEntry.GetSchemaName();
                var tableName = zipEntry.GetTableName();
                var table = tables.Single(x => x.SchemaName == schemaName && x.TableName == tableName);
                var actualTableFile = Export(table, directoryInfo);
                // ReSharper disable once AccessToDisposedClosure
                using var zipStreamReader = new StreamReader(zipFile.GetInputStream(zipEntry), Encoding.UTF8);
                
                // zipとエクスポートしたテキストからテキスト全文を取得する
                var expectedTableText = zipStreamReader.ReadToEnd();
                var actualTableText = File.ReadAllText(actualTableFile.FullName);
                if (expectedTableText.Equals(actualTableText))
                {
                    // 一致
                    return;
                }

                // テキストが不一致の場合、実行ごとに変化のあるセルに対応しつつ期待結果ファイルを作成する
                // 期待値と実際値のそれぞれのCsvReaderを作成する
                using var expectedCsv = new CsvReader(new StringReader(expectedTableText)) { Configuration = { HasHeaderRecord = false } };
                using var actualCsv = new CsvReader(new StringReader(actualTableText)) { Configuration = { HasHeaderRecord = false } };

                // CSVからすべての行を一括で読み取る
                var expectedRecords = expectedCsv.GetRecords<dynamic>().ToArray();
                var actualRecords = actualCsv.GetRecords<dynamic>().ToArray();
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
                    var expectedRecordRecord = (IDictionary<string, object>)expectedRecords[rowNumber];
                    var expectedRecordCells = expectedRecordRecord.Values.Cast<string>().ToArray();
                    var actualRecord = (IDictionary<string, object>)actualRecords[rowNumber];
                    var actualRecordCells = actualRecord.Values.Cast<string>().ToArray();
                    // TODO:列数チェック。現在BCPでCSVエクスポートするときにカンマが入っていると正しい列数にならないため一旦未実施とする

                    // 列ごとに処理する
                    for (var columnNumber = 0; columnNumber < table.Columns.Count; columnNumber++)
                    {
                        var column = table.Columns[columnNumber];
                        var expectedRecordCell = expectedRecordCells[columnNumber];
                        var actualRecordCell = actualRecordCells[columnNumber];
                        if (!column.Compare(expectedRecordCell, actualRecordCell, lifeCycleColumnsArray, timeBeforeStart))
                        {
                            if (expectedRecordCell == Column.TimeAfterStart)
                            {
                                compareResult.AddMismatchedMessage($@"{table} テーブル {rowNumber + 1} 行目の {column.ColumnName} 列が一致しませんでした。DB初期化完了時刻 {timeBeforeStart} 、実際値 {actualRecordCell}。");
                            }
                            else
                            {
                                compareResult.AddMismatchedMessage($@"{table} テーブル {rowNumber + 1} 行目の {column.ColumnName} 列が一致しませんでした。期待値 {expectedRecordCell} 、実際値 {actualRecordCell}。");
                            }
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

            using var tableWriter = new TableWriter(table, directoryInfo);

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
        protected abstract List<Table> GetTables();
    }
}