using System;
using Microsoft.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Net;
using Dapper;
using DbAssertions.SqlServer;
using FluentAssertions;
using Xunit;

namespace DbAssertions.Test.SqlServer
{
    public class SqlDatabaseTest
    {
        private static readonly DateTime TimeBeforeStart = DateTime.Parse("2000/02/02");

        protected readonly SqlDatabase Database = new (new SqlConnectionStringBuilder
        {
            DataSource = "localhost, 1444", 
            InitialCatalog = "AdventureWorks",
            UserID = "sa", 
            Password = "P@ssw0rd!",
            Encrypt = false
        }.ToString());
        protected readonly DbAssertionsConfig Config;

        public SqlDatabaseTest()
        {
            Config = new DbAssertionsConfig();
            Config.AddColumnOperator(null, null, "Person", "Suffix", null, ColumnOperators.HostName);
            Config.AddColumnOperator(null, null, "Person", "FirstName", null, ColumnOperators.Random);
            Config.AddColumnOperator(null, null, "Person", "PersonType", null, ColumnOperators.Ignore);
        }

        [Collection(nameof(SqlDatabaseTest))]
        public class FirstExport : SqlDatabaseTest
        {
            private readonly DirectoryInfo _first = new DirectoryInfo("FirstActual").ReCreate();

            [Fact]
            public void ToBeExported()
            {
                ExecuteNonQuery(@"DatabaseTest\First.sql");
                Database.FirstExport(_first);
                _first.GetDirectory("First")
                    .Should().HaveSameContents(new DirectoryInfo(@"DatabaseTest\FirstExport\ToBeExported"));
            }
        }

        [Collection(nameof(SqlDatabaseTest))]
        public class SecondExport : SqlDatabaseTest
        {
            [Fact]
            public void ToBeExported()
            {
                ExecuteNonQuery(@"DatabaseTest\Second.sql");

                var workDirectory = new DirectoryInfo(@"DatabaseTest\SecondExport\ToBeExported");
                var expectedFile = workDirectory.GetFile("ExpectedAdventureWorks.zip");
                try
                {
                    expectedFile.Delete();
                }
                catch
                {
                    // ignore
                }

                Database.SecondExport(workDirectory, TimeBeforeStart, Config);

                var zipArchive = ZipFile.OpenRead(
                    @"DatabaseTest\SecondExport\ToBeExported\ExpectedAdventureWorks.zip");
                foreach (var entry in zipArchive.Entries)
                {
                    entry.ReadAllBytes()
                        .Should().Equal(
                            File.ReadAllBytes(Path.Combine(@"DatabaseTest\SecondExport\ToBeExported\ExpectedOfExpected",
                                entry.Name)),
                            entry.Name);
                }
            }
        }

        [Collection(nameof(SqlDatabaseTest))]
        public class Compare : SqlDatabaseTest
        {
            [Fact]
            public void Matches()
            {
                ExecuteNonQuery(@"DatabaseTest\CompareMatches.sql");

                var workDirectory = new DirectoryInfo("WorkCompare").ReCreate();

                var compareResult = Database.Compare(
                    new FileInfo(@"DatabaseTest\Compare\ExpectedAdventureWorks.zip"),
                    TimeBeforeStart,
                    Config,
                    workDirectory);

                compareResult.HasMismatched
                    .Should().BeFalse();
            }

            [Fact]
            public void UnMatches()
            {
                ExecuteNonQuery(@"DatabaseTest\CompareUnMatches.sql");

                var workDirectory = new DirectoryInfo("WorkCompare").ReCreate();

                var compareResult = Database.Compare(
                    new FileInfo(@"DatabaseTest\Compare\ExpectedAdventureWorks.zip"),
                    TimeBeforeStart,
                    Config,
                    workDirectory);

                compareResult.HasMismatched
                    .Should().BeTrue();
            }
        }

        private void ExecuteNonQuery(string sqlFile)
        {
            using var connection = new SqlConnection(Database.ConnectionString);
            connection.Open();
            connection.Execute(File.ReadAllText(sqlFile).Replace("%HostName%", Dns.GetHostName()));
        }
    }

    public static class ZipArchiveEntryExtensions
    {
        public static byte[] ReadAllBytes(this ZipArchiveEntry zipArchiveEntry)
        {
            using var stream = zipArchiveEntry.Open();
            var result = new byte[(int)zipArchiveEntry.Length];
            stream.Read(result, 0, result.Length);
            return result;
        }
    }
}