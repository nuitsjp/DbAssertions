using System;
using FluentAssertions;
using Xunit;

namespace DbAssertions.Test
{
    public class IgnoreColumnOperatorTest
    {
        IgnoreColumnOperator Subject { get; } = new();

        public class ToExpected : IgnoreColumnOperatorTest
        {
            private readonly Column _column =
                new("database", "schema", "table", "column", ColumnType.Other, false, 0, new DefaultColumnOperator());

            [Fact]
            public void Where_first_equal_second_Should_return_label()
            {
                Subject.ToExpected(_column, 1, "foo", "foo")
                    .Should().Be("Ignore");
            }
        }

        public class Compare : IgnoreColumnOperatorTest
        {
            [Fact]
            public void Where_actual_equal_expected_Should_be_true()
            {
                Subject.Compare("foo", "foo", DateTime.Now)
                    .Should().BeTrue();
            }

            [Fact]
            public void Where_actual_is_not_expected_or_hostname_Should_be_true()
            {
                Subject.Compare("foo", "bar", DateTime.Now)
                    .Should().BeTrue();
            }
        }

    }
}