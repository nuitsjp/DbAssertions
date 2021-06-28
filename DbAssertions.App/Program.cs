using System;
using System.IO;
using System.Threading.Tasks;
using ConsoleAppFramework;
using Microsoft.Extensions.Hosting;

namespace DbAssertions.App
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
        public void First(
            [Option("s", "サーバー")] string server,
            [Option("d", "データベース")] string database,
            [Option("u", "ユーザー")] string userId,
            [Option("p", "パスワード")] string password,
            [Option("o", "エクスポートディレクトリ")] string output = "output")
        {
            new Database(server, database, userId, password).FirstExport(new DirectoryInfo(output));
        }

        /// <summary>
        /// 2回目のデータのエクスポートと期待結果ファイルの作成
        /// </summary>
        [Command("second")]
        public void Second(
            [Option("s", "サーバー")] string server,
            [Option("d", "データベース")] string database,
            [Option("u", "ユーザー")] string userId,
            [Option("p", "パスワード")] string password,
            [Option("o", "エクスポートディレクトリ")] string output = "output")
        {
            new Database(server, database, userId, password).SecondExport(new DirectoryInfo(output));
        }
    }
}
