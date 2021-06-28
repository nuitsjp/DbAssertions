using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Dapper;
using ICSharpCode.SharpZipLib.Zip;
#if NET40
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif

namespace DbAssertions
{
    /// <summary>
    /// テスト対象のデータベースを表す。
    /// </summary>
    public class Database
    {
        /// <summary>
        /// サーバー
        /// </summary>
        public string Server { get; }

        /// <summary>
        /// データベース
        /// </summary>
        public string DatabaseName { get; }

        /// <summary>
        /// ユーザーID
        /// </summary>
        public string UserId { get; }

        /// <summary>
        /// パスワード
        /// </summary>
        public string Password { get; }

        /// <summary>
        /// インスタンスを初期化する
        /// </summary>
        /// <param name="server"></param>
        /// <param name="databaseName"></param>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        public Database(string server, string databaseName, string userId, string password)
        {
            Server = server;
            DatabaseName = databaseName;
            UserId = userId;
            Password = password;
        }

        /// <summary>
        /// 接続文字列を取得する
        /// </summary>
        public string ConnectionString => new SqlConnectionStringBuilder
        {
            DataSource = Server,
            UserID = UserId,
            Password = Password,
            InitialCatalog = DatabaseName
        }.ToString();

        /// <summary>
        /// 1回目のエクスポートを実行する
        /// </summary>
        /// <param name="directoryInfo"></param>
        public void FirstExport(DirectoryInfo directoryInfo)
        {
            var exportDirectoryInfo = directoryInfo.GetDirectory("First");
            exportDirectoryInfo.ReCreate();

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
                    .GetFiles("*.csv")
                    .Where(x => x.Name.StartsWith($"[{DatabaseName}]"));

            // 並列処理で全テーブル分の処理を実施する
            Parallel.ForEach(tableFiles, firstTableFile =>
            {
                // １回目のファイル名からテーブルオブジェクトを作成する
                var table = Table.Parse(firstTableFile.Name);

                // ２回目のエクスポートを実行する
                var secondTableFile = Export(table, secondDirectoryInfo);

                // ファイルを読み込みテキストを比較する
                var firstTableText = File.ReadAllText(firstTableFile.FullName);
                var secondTableText = File.ReadAllText(secondTableFile.FullName);
                if (firstTableText.Equals(secondTableText))
                {
                    // 完全一致した場合は、１回目のファイルを期待結果ファイルとしてコピーする
                    firstTableFile.CopyTo(expectedDirectoryInfo.GetFile(firstTableFile.Name).FullName);
                    return;
                }

                // テキストが不一致の場合、実行ごとに変化のあるセルに対応しつつ期待結果ファイルを作成する
                // 1回目と2回目のそれぞれのCsvReaderと期待結果のCsvWriterを作成する
                using var firstCsv = new CsvReader(new StringReader(firstTableText)) { Configuration = { HasHeaderRecord = false } };
                using var secondCsv = new CsvReader(new StringReader(secondTableText)) { Configuration = { HasHeaderRecord = false } };
                using var expectedCsv = new CsvWriter(new StreamWriter(File.Open(expectedDirectoryInfo.GetFile(firstTableFile.Name).FullName, FileMode.Create)));

                // 1回目と2回目の全CSV行を読み込む
                var firstRecords = firstCsv.GetRecords<dynamic>().ToArray();
                var secondRecords = secondCsv.GetRecords<dynamic>().ToArray();

                if (firstRecords.Length != secondRecords.Length)
                {
                    // 実行ごとに行数が変わるようなケースは対応しない。
                    // その場合、対象テーブルを除外して個別にテストすること。
                    throw new DbAssertionsException($@"ファイル {firstTableFile.Name} の行数が一致しませんでした。");
                }

                // テーブル列を取得する
                var columns = GetTableColumns(table);

                // 行ごとに処理を実施する
                for (var rowNumber = 0; rowNumber < firstRecords.Length; rowNumber++)
                {
                    // 全列をオブジェクト化する
                    var firstRecord = (IDictionary<string, object>)firstRecords[rowNumber];
                    var firstRecordCells = firstRecord.Values.Cast<string>().ToArray();
                    var secondRecord = (IDictionary<string, object>)secondRecords[rowNumber];
                    var secondRecordCells = secondRecord.Values.Cast<string>().ToArray();

                    if (firstRecordCells.Length != secondRecordCells.Length)
                    {
                        // 列数が異なる場合、テーブル構造が変化しており実施手順に何らかの問題がある
                        // CsvHelperの仕様で1行目の列数が全行取得されるため、必ず1行目でエラーが発生する
                        // 2行目以降の列数エラーは、ここ以降の判定でセルの値の不一致として判断される
                        throw new DbAssertionsException($@"ファイル {firstTableFile.Name} の列数が一致しませんでした。");
                    }

                    // 列ごとに処理する
                    for (var columnNumber = 0; columnNumber < columns.Count; columnNumber++)
                    {
                        var column = columns[columnNumber];
                        var firstRecordCell = firstRecordCells[columnNumber];
                        var secondRecordCell = secondRecordCells[columnNumber];
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
        /// <param name="directoryInfo"></param>
        /// <param name="compareResult"></param>
        /// <param name="setupCompletionTime"></param>
        /// <param name="lifeCycleColumns"></param>
        /// <param name="because"></param>
        /// <param name="becauseArgs"></param>
        public void Compare(
            FileInfo expectedFileInfo,
            DirectoryInfo directoryInfo,
            CompareResult compareResult,
            DateTime setupCompletionTime,
            IEnumerable<LifeCycleColumn> lifeCycleColumns,
            string because = "",
            params object[] becauseArgs)
        {
            // 期待結果zipファイルを展開する
            using var compressStreamA = expectedFileInfo.OpenRead();
            using var zipFile = new ZipFile(compressStreamA);

            var lifeCycleColumnsArray = lifeCycleColumns as LifeCycleColumn[] ?? lifeCycleColumns.ToArray();

            // zipファイルから対象データベースのテーブルファイルを取得し、並列処理する
            Parallel.ForEach(zipFile.GetZipEntries().Where(x => x.Name.StartsWith($"[{DatabaseName}]")), zipEntry =>
            {
                var table = zipEntry.GetTable();
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


                // テーブルの列を取得する
                var columns = GetTableColumns(table);

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
                    for (var columnNumber = 0; columnNumber < columns.Count; columnNumber++)
                    {
                        var column = columns[columnNumber];
                        var expectedRecordCell = expectedRecordCells[columnNumber];
                        var actualRecordCell = actualRecordCells[columnNumber];
                        if (!column.Compare(expectedRecordCell, actualRecordCell, lifeCycleColumnsArray, setupCompletionTime))
                        {
                            compareResult.AddMismatchedMessage($@"{table} テーブル {rowNumber + 1} 行目の {column.ColumnName} 列が一致しませんでした。期待値 {expectedRecordCell} 、実際値 {actualRecordCell}。");
                        }
                    }
                }
            });
        }

        /// <summary>
        /// BCPコマンドを利用し、対象テーブルの値をCSVにエクスポートする。
        /// </summary>
        /// <param name="table"></param>
        /// <param name="directoryInfo"></param>
        /// <returns></returns>
        public FileInfo Export(Table table, DirectoryInfo directoryInfo)
        {
            var fileInfo = directoryInfo.GetFile($"{table}.csv");

            var columns = GetTableColumns(table);
            var primaryKeys = GetPrimaryKeys(table);

            var query = @$"
select
    {string.Join(", ", columns.Select(x => $"[{x.ColumnName}]"))}
from
    {table}
{(primaryKeys.Any() ? "order by" : string.Empty)}
    {string.Join(", ", primaryKeys)}";

            using var connection = new SqlConnection(ConnectionString);
            connection.Open();

            using var expectedCsv = new CsvWriter(new StreamWriter(fileInfo.Open(FileMode.Create)));

            using var command = new SqlCommand(query, connection);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                foreach (var column in columns)
                {
                    var value = reader[column.ColumnName];
                    if (value == DBNull.Value)
                    {
                        expectedCsv.WriteField(null);
                    }
                    else if (column.ColumnType == ColumnType.VarBinary)
                    {
                        using var sha256 = SHA256.Create();
                        var hash = sha256.ComputeHash((byte[])value);
                        expectedCsv.WriteField(string.Concat(hash.Select(b => $"{b:x2}")));
                    }
                    else
                    {
                        expectedCsv.WriteField(value);
                    }
                }
                expectedCsv.NextRecord();
            }

            return fileInfo;
        }

        /// <summary>
        /// データベースのすべてのユーザーテーブルを取得する。
        /// </summary>
        /// <returns></returns>
        private IList<Table> GetTables()
        {
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();

            var query = @$"
use {DatabaseName};

select 
	'{DatabaseName}' as [DatabaseName],
	schema_name(schema_id) as [SchemaName],
	name as [TableName]
from 
	sys.objects 
where 
	type = 'U'
order by 
	[DatabaseName],
	[SchemaName],
	[TableName];";
            return connection.Query<Table>(query).ToList();
        }

        /// <summary>
        /// テーブルの列を取得する。
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public IList<Column> GetTableColumns(Table table)
        {
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();

            var query = @"
select
	schemas.name as SchemaName,
	columns.name as ColumnName,
	system_type_id as SystemTypeId
from
	sys.columns
	inner join sys.tables
		on	columns.object_id = tables.object_id
	inner join sys.schemas
		on	tables.schema_id = schemas.schema_id
where
	tables.name = @TableName
order by
	column_id
";

            return connection
                .Query(
                    query,
                    new { TableName = table.TableName })
                .Select(x =>
                {
                    return new Column(
                        DatabaseName,
                        x.SchemaName,
                        table.TableName,
                        (string)x.ColumnName,
                        (byte)x.SystemTypeId switch
                        {
                            (byte)ColumnType.VarBinary => ColumnType.VarBinary,
                            (byte)ColumnType.DateTime => ColumnType.DateTime,
                            _ => ColumnType.Other
                        });
                })
                .ToList();
        }

        public IList<string> GetPrimaryKeys(Table table)
        {
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();

            var query = @"
select
	columns.name as Name
from
	sys.tables
	inner join sys.key_constraints
		on tables.object_id = key_constraints.parent_object_id AND key_constraints.type = 'PK'
	inner join sys.index_columns
		on key_constraints.parent_object_id = index_columns.object_id
		and key_constraints.unique_index_id  = index_columns.index_id
	inner join sys.columns
		on index_columns.object_id = columns.object_id
		and index_columns.column_id = columns.column_id
where
	tables.name = @TableName
order by
	index_columns.key_ordinal";

            return connection
                .Query(
                    query,
                    new {TableName = table.TableName})
                .Select(x => (string)x.Name)
                .ToList();
        }
    }
}