using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
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

        protected readonly SqlDatabase Database;
        protected readonly DbAssertionsConfig Config;

        private readonly AdventureWorks _adventureWorks;



        public SqlDatabaseTest()
        {
            Config = new DbAssertionsConfig();
            Config.AddColumnOperator(null, null, "Person", "Suffix", null, ColumnOperators.HostName);
            Config.AddColumnOperator(null, null, "Person", "FirstName", null, ColumnOperators.Random);
            Config.AddColumnOperator(null, null, "Person", "PersonType", null, ColumnOperators.Ignore);
            HostNameColumnOperator.HostNameProvider = new HostNameProviderStub();

            _adventureWorks = AdventureWorks.Start();

            Database = new(new SqlConnectionStringBuilder
            {
                DataSource = $"localhost, {_adventureWorks.Port}",
                InitialCatalog = "AdventureWorks",
                UserID = "sa",
                Password = AdventureWorks.SaPassword,
                Encrypt = false
            }.ToString());
        }

        public void Dispose()
        {
            HostNameColumnOperator.HostNameProvider = new HostNameProvider();
            _adventureWorks.Dispose();
        }

        public class HostNameProviderStub : IHostNameProvider
        {
            public string GetHostName() => HostName;
        }

        public class FirstExport : SqlDatabaseTest
        {
            private readonly DirectoryInfo _first = new DirectoryInfo("FirstActual").ReCreate();

            [Fact]
            public async Task ToBeExported()
            {
                ExecuteNonQuery(@"DatabaseTest\First.sql");
                Database.FirstExport(_first);
                _first.GetDirectory("First")
                    .Should().HaveSameContents(new DirectoryInfo(@"DatabaseTest\FirstExport\ToBeExported"));
            }
        }

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

            using var connection = OpenConnection(Database.ConnectionString);
            connection.Execute(query);
        }

        private IDbConnection OpenConnection(string connectionString)
        {
            IDbConnection? connection = null;
            for (var i = 0; ; i++)
            {
                try
                {
                    connection = new SqlConnection(Database.ConnectionString);
                    connection.Open();
                    return connection;
                }
                catch
                {
                    connection?.Dispose();
                    if (i == 100)
                    {
                        throw;
                    }
                    Thread.Sleep(TimeSpan.FromMilliseconds(200));
                }
            }
        }
    }
}