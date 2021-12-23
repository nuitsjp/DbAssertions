using System;
#if NETFRAMEWORK
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ConsoleAppFramework;
using Microsoft.Extensions.Hosting;

namespace DbAssertions.SqlServer.App
{
    class Program : ConsoleAppBase
    {
        static async Task Main(string[] args)
        {
            await Host
                .CreateDefaultBuilder()
                .RunConsoleAppFrameworkAsync<Program>(args);
        }

        /// <summary>
        /// １回目のデータのエクスポート
        /// </summary>
        [Command("first")]
        // ReSharper disable once UnusedMember.Global
        public void First(
            [Option("s", "サーバー")] string server,
            [Option("d", "データベース")] string database,
            [Option("u", "ユーザー")] string userId,
            [Option("p", "パスワード")] string password,
            [Option("o", "エクスポートディレクトリ")] string output = "output")
        {

            new SqlDatabase(
                new SqlConnectionStringBuilder
                {
                    DataSource = server,
                    UserID = userId,
                    Password = password,
                    InitialCatalog = database,
                    Encrypt = false
                }.ToString()).FirstExport(new DirectoryInfo(output));
        }

        /// <summary>
        /// 2回目のデータのエクスポートと期待結果ファイルの作成
        /// </summary>
        [Command("second")]
        // ReSharper disable once UnusedMember.Global
        public void Second(
            [Option("s", "サーバー")] string server,
            [Option("d", "データベース")] string database,
            [Option("u", "ユーザー")] string userId,
            [Option("p", "パスワード")] string password,
            [Option("i", "初期化完了時刻")] string initializedDateTime,
            [Option("o", "エクスポートディレクトリ")] string output = "output")
        {
            using var processModule = Process.GetCurrentProcess().MainModule!;
            var configPath = Path.Combine(Path.GetDirectoryName(processModule.FileName)!, "DbAssertions.json");
            if (File.Exists(configPath))
            {
                var config = DbAssertionsConfig.Deserialize(configPath);
                new SqlDatabase(
                    new SqlConnectionStringBuilder
                    {
                        DataSource = server,
                        UserID = userId,
                        Password = password,
                        InitialCatalog = database,
                        Encrypt = false
                    }.ToString()).SecondExport(new DirectoryInfo(output), DateTime.Parse(initializedDateTime), config);
            }
            else
            {
                new SqlDatabase(
                    new SqlConnectionStringBuilder
                    {
                        DataSource = server,
                        UserID = userId,
                        Password = password,
                        InitialCatalog = database,
                        Encrypt = false
                    }.ToString()).SecondExport(new DirectoryInfo(output), DateTime.Parse(initializedDateTime));
            }
        }
    }
}