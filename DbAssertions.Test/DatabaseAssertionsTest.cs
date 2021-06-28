using System;
using System.IO;
using DbAssertions.SqlServer;
using FluentAssertions;
using Xunit;

namespace DbAssertions.Test
{
    public class DatabaseAssertionsTest
    {
        public class BeAsExpected
        {
            private readonly Database _database = new SqlDatabase("localhost, 1444", "AdventureWorks", "sa", "P@ssw0rd!");

            [Fact]
            public void Matches()
            {
                var workDirectory = new DirectoryInfo("WorkBeAsExpected").ReCreate();

                var compareResult = new CompareResult();
                _database.Should().BeAsExpected(
                    new FileInfo(@"DatabaseTest\Compare\ExpectedAdventureWorks.zip"),
                    workDirectory,
                    compareResult,
                    DateTime.Parse("2011/05/30 0:00:00"),
                    new[] { new LifeCycleColumn(null, null, "SalesOrderDetail", "ModifiedDate", LifeCycle.Runtime) });

                compareResult.HasMismatched
                    .Should().BeFalse();
            }

            [Fact]
            public void UnMatches()
            {
                var workDirectory = new DirectoryInfo("WorkBeAsExpected").ReCreate();

                var compareResult = new CompareResult();
                _database.Should().BeAsExpected(
                    new FileInfo(@"DatabaseTest\Compare\ExpectedAdventureWorks.zip"),
                    workDirectory,
                    compareResult,
                    DateTime.Parse("2011/05/31 0:00:01"));

                compareResult.HasMismatched
                    .Should().BeTrue();
            }

        }
    }
}