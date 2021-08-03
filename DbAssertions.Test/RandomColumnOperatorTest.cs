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
            public void When_first_equal_second_Should_return_first()
            {
                Subject.ToExpected(_column, 1, "foo", "foo")
                    .Should().Be("foo");
            }

            [Fact]
            public void When_first_not_equal_second_Should_return_label()
            {
                Subject.ToExpected(_column, 1, "foo", "bar")
                    .Should().Be(RandomColumnOperator.DefaultLabel);
            }
        }

        public class Compare : RandomColumnOperatorTest
        {
            [Fact]
            public void When_expected_is_label_Should_return_true()
            {
                Subject.Compare("Random", "bar", DateTime.Now)
                    .Should().BeTrue();
            }

            [Fact]
            public void When_actual_equal_expected_Should_return_true()
            {
                Subject.Compare("foo", "foo", DateTime.Now)
                    .Should().BeTrue();
            }

            [Fact]
            public void When_actual_not_equal_expected_Should_return_true()
            {
                Subject.Compare("foo", "bar", DateTime.Now)
                    .Should().BeFalse();
            }
        }
    }
}