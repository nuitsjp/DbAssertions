using System;
using System.Configuration;
using System.Net;
using FluentAssertions;
using Xunit;

namespace DbAssertions.Test
{
    public class SetupTimeColumnOperatorTest
    {
        SetupTimeColumnOperator Subject { get; } = new();

        public class ToExpected : SetupTimeColumnOperatorTest
        {
            private readonly Column _column =
                new("database", "schema", "table", "column", ColumnType.Other, false, 0, new DefaultColumnOperator());

            [Fact]
            public void Where_first_equal_second_Should_return_first()
            {
                Subject.ToExpected(_column, 1, "2021/01/01", "2021/01/01")
                    .Should().Be("2021/01/01");
            }

            [Fact]
            public void Where_first_before_second_Should_return_label()
            {
                Subject.ToExpected(_column, 1, "2021/01/01", "2021/01/02")
                    .Should().Be(SetupTimeColumnOperator.TimeBeforeStart);

            }


            [Fact]
            public void Where_first_after_second_Should_thrown_exception()
            {
                Subject.Invoking(x => x.ToExpected(_column, 1, "2021/01/02", "2021/01/01"))
                    .ShouldThrow<DbAssertionsException>();

            }
        }

        public class Compare : SetupTimeColumnOperatorTest
        {
            [Fact]
            public void Where_actual_equal_expected_Should_be_true()
            {
                Subject.Compare("2021/01/01", "2021/01/01", DateTime.Now)
                    .Should().BeTrue();
            }

            [Fact]
            public void Where_actual_before_start_time_Should_be_true()
            {
                Subject.Compare(SetupTimeColumnOperator.TimeBeforeStart, "2021/01/01", DateTime.Parse("2021/01/02"))
                    .Should().BeTrue();
            }

            [Fact]
            public void Where_actual_after_start_time_Should_be_false()
            {
                Subject.Compare(SetupTimeColumnOperator.TimeBeforeStart, "2021/01/02", DateTime.Parse("2021/01/01"))
                    .Should().BeFalse();
            }
        }
    }
}