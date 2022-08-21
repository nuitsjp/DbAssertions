using System;
#if NETFRAMEWORK
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using ConsoleAppFramework;
using Microsoft.Extensions.Hosting;
using Microsoft.SqlServer.Types;

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
        /// First database export.
        /// </summary>
        [Command("first")]
        // ReSharper disable once UnusedMember.Global
        public void First(
            [Option("s", "Server")] string server,
            [Option("d", "Database")] string database,
            [Option("u", "User ID")] string userId,
            [Option("p", "Password")] string password,
            [Option("o", "Export directory")] string output = "output")
        {
            new SqlDatabase(
                new SqlConnectionStringBuilder
                {
                    DataSource = server,
                    UserID = userId,
                    Password = password,
                    InitialCatalog = database,
                    Encrypt = false
                }.ToString(),
                ColumnOperatorProvider.Default).FirstExport(new DirectoryInfo(output));
        }

        /// <summary>
        /// 2回目のデータのエクスポートと期待結果ファイルの作成
        /// </summary>
        [Command("second")]
        // ReSharper disable once UnusedMember.Global
        public void Second(
            [Option("s", "Server")] string server,
            [Option("d", "Database")] string database,
            [Option("u", "User ID")] string userId,
            [Option("p", "Password")] string password,
            [Option("i", "Database initialization completion time")] string initializedDateTime,
            [Option("o", "Export directory")] string output = "output")
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
                    }.ToString(),
                    ColumnOperatorProvider.Default).SecondExport(new DirectoryInfo(output), DateTime.Parse(initializedDateTime), config);
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
                    }.ToString(),
                    ColumnOperatorProvider.Default).SecondExport(new DirectoryInfo(output), DateTime.Parse(initializedDateTime));
            }
        }
    }
}