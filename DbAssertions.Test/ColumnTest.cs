using System;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace DbAssertions.Test
{
    public class ColumnTest
    {
        public class ToExpected
        {
            [Fact]
            public void When_equal_Should_return_value()
            {
                Column column = new("database", "schema", "table", "column", ColumnType.Other);
                var value = "value";
                column.ToExpected(value, value, 1)
                    .Should().Be(value);
            }

            [Fact]
            public void When_not_equal_and_Other_Should_throw_exception()
            {
                Column column = new("database", "schema", "table", "column", ColumnType.Other);
                var value = "value";
                column.Invoking(x => x.ToExpected(value, "not value", 1))
                    .Should().Throw<DbAssertionsException>();
            }


            [Fact]
            public void When_not_equal_and_VarBinary_Should_throw_exception()
            {
                Column column = new("database", "schema", "table", "column", ColumnType.VarBinary);
                var value = "value";
                column.Invoking(x => x.ToExpected(value, "not value", 1))
                    .Should().Throw<DbAssertionsException>();
            }

            [Fact]
            public void When_first_is_empty_and_DateTime_Should_throw_exception()
            {
                Column column = new("database", "schema", "table", "column", ColumnType.DateTime);
                column.Invoking(x => x.ToExpected(string.Empty, "not empty", 1))
                    .Should().Throw<DbAssertionsException>();
            }

            [Fact]
            public void When_second_is_empty_and_DateTime_Should_throw_exception()
            {
                Column column = new("database", "schema", "table", "column", ColumnType.DateTime);
                column.Invoking(x => x.ToExpected(string.Empty, "not empty", 1))
                    .Should().Throw<DbAssertionsException>();
            }

            [Fact]
            public void When_first_is_before_second_Should_return_TimeAfterStart()
            {
                Column column = new("database", "schema", "table", "column", ColumnType.DateTime);
                column.ToExpected("2000/01/01", "2000/01/02", 1)
                    .Should().Be(Column.TimeAfterStart);

            }

            [Fact]
            public void When_first_is_after_second_Should_throw_exception()
            {
                Column column = new("database", "schema", "table", "column", ColumnType.DateTime);
                column.Invoking(x => x.ToExpected("2000/01/02", "2000/01/01", 1))
                    .Should().Throw<DbAssertionsException>();

            }
        }

        public class Compare
        {
            [Fact]
            public void When_equal_Should_be_true()
            {
                Column column = new("database", "schema", "table", "column", ColumnType.Other);
                var value = "value";
                column.Compare(value, value, Enumerable.Empty<LifeCycleColumn>(), DateTime.MaxValue)
                    .Should().BeTrue();
            }

            [Fact]
            public void When_not_equal_Should_be_false()
            {
                Column column = new("database", "schema", "table", "column", ColumnType.Other);
                column.Compare("value1", "value2", Enumerable.Empty<LifeCycleColumn>(), DateTime.MaxValue)
                    .Should().BeFalse();
            }

            [Fact]
            public void When_LifeCycleColumn_and_expected_is_empty_Should_be_false()
            {
                Column column = new("database", "schema", "table", "column", ColumnType.DateTime);
                LifeCycleColumn[] lifeCycleColumns =
                    {new("database", "schema", "table", "column", LifeCycle.Daily)};

                column.Compare(string.Empty, "value", lifeCycleColumns, DateTime.MaxValue)
                    .Should().BeFalse();
            }

            [Fact]
            public void When_LifeCycleColumn_and_actual_is_empty_Should_be_false()
            {
                Column column = new("database", "schema", "table", "column", ColumnType.DateTime);
                LifeCycleColumn[] lifeCycleColumns =
                    {new("database", "schema", "table", "column", LifeCycle.Daily)};

                column.Compare("value", string.Empty, lifeCycleColumns, DateTime.MaxValue)
                    .Should().BeFalse();
            }

            [Fact]
            public void When_LifeCycle_is_Daily_and_DateTime_format_Should_be_true()
            {
                Column column = new("database", "schema", "table", "column", ColumnType.DateTime);
                LifeCycleColumn[] lifeCycleColumns =
                    {new("database", "schema", "table", "column", LifeCycle.Daily)};

                column.Compare("value", "2000/01/01", lifeCycleColumns, DateTime.MaxValue)
                    .Should().BeTrue();
            }

            [Fact]
            public void When_LifeCycle_is_Daily_and_not_DateTime_format_Should_be_false()
            {
                Column column = new("database", "schema", "table", "column", ColumnType.DateTime);
                LifeCycleColumn[] lifeCycleColumns =
                    {new("database", "schema", "table", "column", LifeCycle.Daily)};

                column.Compare("foo", "bar", lifeCycleColumns, DateTime.MaxValue)
                    .Should().BeFalse();
            }

            [Fact]
            public void When_LifeCycle_is_Runtime_and_expected_is_TimeAfterStart_and_actual_after_timeBeforeStart_Should_be_true()
            {
                Column column = new("database", "schema", "table", "column", ColumnType.DateTime);
                LifeCycleColumn[] lifeCycleColumns = 
                    {new("database", "schema", "table", "column", LifeCycle.Runtime)};

                var actual = DateTime.Now;
                var timeBeforeStart = actual.AddMinutes(-1);

                column.Compare(Column.TimeAfterStart, actual.ToString(CultureInfo.InvariantCulture), lifeCycleColumns, timeBeforeStart)
                    .Should().BeTrue();

            }


            [Fact]
            public void When_LifeCycle_is_Runtime_and_expected_is_TimeAfterStart_and_actual_before_timeBeforeStart_Should_be_false()
            {
                Column column = new("database", "schema", "table", "column", ColumnType.DateTime);
                LifeCycleColumn[] lifeCycleColumns =
                    {new("database", "schema", "table", "column", LifeCycle.Runtime)};

                var actual = DateTime.Now;
                var timeBeforeStart = actual.AddMinutes(1);

                column.Compare(Column.TimeAfterStart, actual.ToString(CultureInfo.InvariantCulture), lifeCycleColumns, timeBeforeStart)
                    .Should().BeFalse();

            }

            [Fact]
            public void When_LifeCycle_is_Runtime_and_expected_is_not_TimeAfterStart_Should_be_false()
            {
                Column column = new("database", "schema", "table", "column", ColumnType.DateTime);
                LifeCycleColumn[] lifeCycleColumns =
                    {new("database", "schema", "table", "column", LifeCycle.Runtime)};

                var actual = DateTime.Now;
                var timeBeforeStart = actual.AddMinutes(-1);

                column.Compare("2000/01/01", actual.ToString(CultureInfo.InvariantCulture), lifeCycleColumns, timeBeforeStart)
                    .Should().BeFalse();

            }
        }
    }
}
