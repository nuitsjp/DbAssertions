using System;
using System.IO;
using System.IO.Compression;
using FluentAssertions;
using Xunit;

namespace DbAssertions.Test
{
    public class DatabaseTest
    {
        protected readonly Database Database = new("localhost, 1444", "AdventureWorks", "sa", "P@ssw0rd!");
        public class FirstExport : DatabaseTest
        {
            private readonly DirectoryInfo _first = new DirectoryInfo("FirstActual").ReCreate();

            [Fact]
            public void ToBeExported()
            {
                Database.FirstExport(_first);
                _first.GetDirectory("First")
                    .Should().HaveSameContents(new DirectoryInfo(@"DatabaseTest\FirstExport\ToBeExported"));
            }
        }

        public class SecondExport : DatabaseTest
        {
            [Fact]
            public void ToBeExported()
            {
                var workDirectory = new DirectoryInfo(@"DatabaseTest\SecondExport\ToBeExported");
                var secondDirectory = workDirectory.GetDirectory("Second").ForceDelete();
                var expectedDirectory = workDirectory.GetDirectory("Expected").ForceDelete();
                var expectedFile = workDirectory.GetFile("ExpectedAdventureWorks.zip");
                try
                {
                    expectedFile.Delete();
                }
                catch
                {
                    // ignore
                }

                Database.SecondExport(workDirectory);

                var zipArchive = ZipFile.OpenRead(
                    @"DatabaseTest\SecondExport\ToBeExported\ExpectedAdventureWorks.zip");
                foreach (var entry in zipArchive.Entries)
                {
                    entry.ReadAllBytes()
                        .Should().Equal(
                            File.ReadAllBytes(Path.Combine(@"DatabaseTest\SecondExport\ToBeExported\ExpectedOfExpected",
                                entry.Name)));
                }
            }
        }

        public class Compare : DatabaseTest
        {
            [Fact]
            public void Matches()
            {
                var workDirectory = new DirectoryInfo("WorkCompare").ReCreate();

                var compareResult = new CompareResult();
                Database.Compare(
                    new FileInfo(@"DatabaseTest\Compare\ExpectedAdventureWorks.zip"),
                    workDirectory,
                    compareResult,
                    DateTime.Parse("2011/05/30 0:00:00"),
                    new []{new LifeCycleColumn(null, null, "SalesOrderDetail", "ModifiedDate", LifeCycle.Runtime) });

                compareResult.HasMismatched
                    .Should().BeFalse();
            }

            [Fact]
            public void UnMatches()
            {
                var workDirectory = new DirectoryInfo("WorkCompare").ReCreate();

                var compareResult = new CompareResult();
                Database.Compare(
                    new FileInfo(@"DatabaseTest\Compare\ExpectedAdventureWorks.zip"),
                    workDirectory,
                    compareResult,
                    DateTime.Parse("2011/05/31 0:00:01"),
                    new[] { new LifeCycleColumn(null, null, "SalesOrderDetail", "ModifiedDate", LifeCycle.Runtime) });

                compareResult.HasMismatched
                    .Should().BeTrue();
            }
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