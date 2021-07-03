using System.IO;
using FluentAssertions;
using FluentAssertions.Primitives;

namespace DbAssertions
{
    public static class DatabaseExtensions
    {
        public static DatabaseAssertions Should(this Database instance)
        {
            return new(instance);
        }

    }

    public class DatabaseAssertions : ReferenceTypeAssertions<Database, DatabaseAssertions>
    {
        public DatabaseAssertions(Database instance)
        {
            Subject = instance;
        }

        protected override string Context => "database";

        //public AndConstraint<DatabaseAssertions> BeExpected(
        //    DirectoryInfo expected,
        //    bool ignoreNumbersInFileName = false,
        //    string because = "",
        //    params object[] becauseArgs)
        //{
        //    HasSameFiles(Subject, expected, ignoreNumbersInFileName, because, becauseArgs);
        //    return new AndConstraint<DatabaseAssertions>(this);
        //}
    }
}