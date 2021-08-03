using System;
using FluentAssertions;
using Xunit;

namespace DbAssertions.Test
{
    public class RandomColumnOperatorTest
    {
        RandomColumnOperator Subject { get; } = new();

        public class ToExpected : RandomColumnOperatorTest
        {
            private readonly Column _column =
                new("database", "schema", "table", "column", ColumnType.Other, false, 0, new DefaultColumnOperator());

            [Fact]
            public void Should_return_label()
            {
                Subject.ToExpected(_column, 1, "foo", "bar")
                    .Should().Be("Random");
            }
        }

        public class Compare : RandomColumnOperatorTest
        {
            [Fact]
            public void Should_return_true()
            {
                Subject.Compare("foo", "bar", DateTime.Now)
                    .Should().BeTrue();
            }
        }
    }
}