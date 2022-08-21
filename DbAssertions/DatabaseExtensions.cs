using System;
using System.IO;
using System.Text;
using FluentAssertions.Execution;
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
        public DatabaseAssertions(Database instance) : base(instance)
        {
        }

        protected override string Identifier => "database";

        public DatabaseAssertions BeExpected(
            DirectoryInfo expected,
            DateTime setupCompletionTime,
            IDbAssertionsConfig? config = null,
            string because = "",
            params object[] becauseArgs)
        {
            var temporary = Path.GetTempFileName();
            File.Delete(temporary);
            try
            {
                Directory.CreateDirectory(temporary);

                var result =
                    Subject.Compare(expected, setupCompletionTime, config ?? new DbAssertionsConfig(), new DirectoryInfo(temporary), because, becauseArgs);

                if (result.HasMismatched)
                {
                    var builder = new StringBuilder();
                    builder.Append(
                        string.IsNullOrEmpty(because)
                            ? "There is a discrepancy in the database."
                            : string.Format(because, becauseArgs));
                    builder.Append(Environment.NewLine);
                    foreach (var message in result.MismatchedMessages)
                    {
                        builder.Append(" - ");
                        builder.Append(message);
                        builder.Append(Environment.NewLine);
                    }

                    throw new AssertionFailedException(builder.ToString());
                }

                return this;
            }
            finally
            {
                Directory.Delete(temporary, true);
            }
        }
    }
}