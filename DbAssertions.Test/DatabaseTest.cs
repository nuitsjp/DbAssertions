using System;
using System.IO;
using Xunit;

namespace DbAssertions.Test
{
    public class DatabaseTest
    {
        protected readonly Database Database = new("localhost, 1444", "AdventureWorks", "sa", "P@ssw0rd!");
        public class First : DatabaseTest
        {
            private readonly DirectoryInfo _first = new DirectoryInfo("FirstActual").ReCreate();

            [Fact]
            public void ToBeExported()
            {
                Database.FirstExport(_first);
                _first.GetDirectory("First")
                    .Should().HaveSameContents(new DirectoryInfo(@"DatabaseTest\First\ToBeExported"));
            }
        }
    }
}