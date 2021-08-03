using System;
using FluentAssertions;
using Xunit;

namespace DbAssertions.Test
{
    public class DefaultColumnOperatorTest
    {
        DefaultColumnOperator Subject { get; } = new();

        public class ToExpected : DefaultColumnOperatorTest
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
            public void When_first_not_equal_second_Should_throw_exception()
            {
                Subject.Invoking(x => x.ToExpected(_column, 1, "foo", "bar"))
                    .ShouldThrow<DbAssertionsException>();
            }
        }

        public class Compare : DefaultColumnOperatorTest
        {
            [Fact]
            public void When_first_equal_second_Should_return_true()
            {
                Subject.Compare("foo", "foo", DateTime.Now)
                    .Should().BeTrue();
            }


            [Fact]
            public void When_first_not_equal_second_Should_return_false()
            {
                Subject.Compare("foo", "bar", DateTime.Now)
                    .Should().BeFalse();
            }
        }
    }
}