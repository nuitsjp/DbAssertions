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