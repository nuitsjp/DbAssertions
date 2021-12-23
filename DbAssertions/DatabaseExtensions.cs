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
    }
}