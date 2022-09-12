using System;
using System.Data;

#if NET461
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif
using System.IO;
using System.Threading;
using Dapper;
using DbAssertions.SqlServer;
using FluentAssertions;
using Xunit;

namespace DbAssertions.Test.SqlServer
{
    public class SqlDatabaseTest : IDisposable
    {
        private static readonly DateTime TimeBeforeStart = DateTime.Parse("2000/02/02");

        private static readonly string HostName = "DummyHost";

        protected readonly Database Database;
        protected readonly DbAssertionsConfig Config;

        private readonly AdventureWorks _adventureWorks;



        public SqlDatabaseTest()
        {
            Config = new DbAssertionsConfig();
            Config.AddColumnOperator(null, null, "Person", "Suffix", null, ColumnOperatorProvider.Default.HostName);
            Config.AddColumnOperator(null, null, "Person", "FirstName", null, ColumnOperatorProvider.Default.Random);
            Config.AddColumnOperator(null, null, "Person", "PersonType", null, ColumnOperatorProvider.Default.Ignore);
            Config.AddColumnOperator(null, null, "DatabaseLog", "XmlEvent", null, ColumnOperatorProvider.Default.Ignore);
#if NET6_0
            Config.AddColumnOperator(null, null, "JobCandidate", "Resume", null, ColumnOperatorProvider.Default.Ignore);
            Config.AddColumnOperator(null, null, "Address", "SpatialLocation", null, ColumnOperatorProvider.Default.Ignore);
#endif

            _adventureWorks = AdventureWorks.Start();

            Database = new SqlDatabase(new SqlConnectionStringBuilder
                {
                    DataSource = $"localhost, {_adventureWorks.Port}",
                    InitialCatalog = "AdventureWorks",
                    UserID = "sa",
                    Password = AdventureWorks.SaPassword,
                    Encrypt = false
                }.ToString(),
                new ColumnOperatorProvider(
                    new HostNameColumnOperatorStub(),
                    new RandomColumnOperator(),
                    new IgnoreColumnOperator()));
        }

        public void Dispose()
        {
            _adventureWorks.Dispose();
        }

        public class FirstShouldBeExported : SqlDatabaseTest
        {
            private readonly DirectoryInfo _first = new DirectoryInfo("FirstActual").ReCreate();

            [Fact]
            public void Invoke()
            {
#if NET461
                var expectedPath = @"DatabaseTest\FirstExport\NetFramework461";
#else
                var expectedPath = @"DatabaseTest\FirstExport\Net6";
#endif

                Database.DatabaseName.Should().Be("AdventureWorks");
                Database.ConnectionString.Should().Be($"Data Source=\"localhost, {_adventureWorks.Port}\";Initial Catalog=AdventureWorks;User ID=sa;Password=P@ssw0rd!;Encrypt=False");

                ExecuteNonQuery(@"DatabaseTest\First.sql");
                Database.FirstExport(_first);
                _first.GetDirectory("First")
                    .Should().HaveSameContents(new DirectoryInfo(expectedPath));
            }
        }

        public class SecondShouldBeExported : SqlDatabaseTest
        {
            [Fact]
            public void Invoke()
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

        public class CompareShouldBeMatches : SqlDatabaseTest
        {
            [Fact]
            public void Invoke()
            {
                ExecuteNonQuery(@"DatabaseTest\CompareMatches.sql");

                var workDirectory = new DirectoryInfo("CompareShouldBeMatches").ReCreate();

                var compareResult = Database.Compare(
                    new DirectoryInfo(@"DatabaseTest\Compare"),
                    TimeBeforeStart,
                    Config,
                    workDirectory);

                compareResult
                    .HasMismatched.Should().BeFalse();
            }
        }

        public class CompareShouldBeUnmatches : SqlDatabaseTest
        {
            [Fact]
            public void Invoke()
            {
                ExecuteNonQuery(@"DatabaseTest\CompareUnMatches.sql");

                var workDirectory = new DirectoryInfo("CompareShouldBeUnmatches").ReCreate();

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

            using var connection = OpenConnection();
            connection.Execute(query);
        }

        /// <summary>
        /// Immediately after docker startup, connection errors occur. Therefore, retry processing is performed.
        /// </summary>
        /// <returns></returns>
        private IDbConnection OpenConnection()
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

        public class HostNameColumnOperatorStub : IColumnOperator
        {
            public const string DefaultLabel = "HostName";

            public string ToExpected(Column column, int rowNumber, string firstCell, string secondCell)
            {
                if (Equals(firstCell, secondCell))
                {
                    if (Equals(firstCell, HostName))
                    {
                        return DefaultLabel;
                    }

                    return firstCell;
                }

                throw DbAssertionsException.FromUnableToExpected(column, rowNumber, firstCell, secondCell);
            }

            public bool Compare(string expectedCell, string actualCell, DateTime timeBeforeStart)
            {
                if (Equals(actualCell, expectedCell))
                {
                    return true;
                }

                if (Equals(actualCell, HostName))
                {
                    return true;
                }

                return false;
            }
        }
    }
}