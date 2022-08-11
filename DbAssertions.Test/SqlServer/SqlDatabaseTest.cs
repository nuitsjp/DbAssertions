﻿using System;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Net;
using Dapper;
using DbAssertions.SqlServer;
using FluentAssertions;
using Xunit;

namespace DbAssertions.Test.SqlServer
{
    public class SqlDatabaseTest : IDisposable
    {
        private static readonly DateTime TimeBeforeStart = DateTime.Parse("2000/02/02");

        private static readonly string HostName = "ZRFG050111";

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
            HostNameColumnOperator.HostNameProvider = new HostNameProviderStub();
        }

        public void Dispose()
        {
            HostNameColumnOperator.HostNameProvider = new HostNameProvider();
        }

        public class HostNameProviderStub : IHostNameProvider
        {
            public string GetHostName() => HostName;
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

                foreach (var tableFile in workDirectory.GetFiles())
                {
                    var actual = File.ReadAllText(tableFile.FullName);
                    var expected = File.ReadAllText(Path.Combine(@"DatabaseTest\SecondExport\ToBeExported\ExpectedOfExpected", tableFile.Name));
                    actual.Should().Be(expected, tableFile.Name);
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
                    new DirectoryInfo(@"DatabaseTest\Compare"),
                    TimeBeforeStart,
                    Config,
                    workDirectory);

                compareResult
                    .HasMismatched.Should().BeFalse();
            }

            [Fact]
            public void UnMatches()
            {
                ExecuteNonQuery(@"DatabaseTest\CompareUnMatches.sql");

                var workDirectory = new DirectoryInfo("WorkCompare").ReCreate();

                var compareResult = Database.Compare(
                    new DirectoryInfo(@"DatabaseTest\Compare"),
                    TimeBeforeStart,
                    Config,
                    workDirectory);

                compareResult
                    .HasMismatched.Should().BeTrue();
            }
        }

        private void ExecuteNonQuery(string sqlFile)
        {
            var query = File.ReadAllText(sqlFile).Replace("%HostName%", HostName);

            using var connection = new SqlConnection(Database.ConnectionString);
            connection.Open();
            connection.Execute(query);
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