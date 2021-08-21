using System;
using System.Net;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace DbAssertions.Test
{
    public class ColumnTest
    {
        public class ToExpected
        {
            [Fact]
            public void When_second_equal_first_Should_return_first()
            {
                var columnOperator = new Mock<IColumnOperator>();
                var column =
                    new Column(
                        "database",
                        "schema",
                        "table",
                        "column",
                        ColumnType.DateTime,
                        true,
                        1,
                        columnOperator.Object);
                int rowNumber = 1;
                var firstCell = "foo";
                var secondCell = firstCell;
                var initializedDateTime = DateTime.Parse("1999/12/31");
                columnOperator
                    .Setup(x => x.ToExpected(column, rowNumber, firstCell, secondCell))
                    .Throws(new AssertionFailedException(string.Empty));

                column.ToExpected(firstCell, secondCell, rowNumber, initializedDateTime)
                    .Should().Be(firstCell);

            }

            [Fact]
            public void When_value_is_not_equal_Should_return_column_operator_result()
            {
                var columnOperator = new Mock<IColumnOperator>();
                var column = 
                    new Column(
                        "database", 
                        "schema", 
                        "table", 
                        "column", 
                        ColumnType.Other, 
                        true, 
                        1, 
                        columnOperator.Object);
                int rowNumber = 1;
                var firstCell = "foo";
                var secondCell = "bar";
                var result = "result";
                var initializedDateTime = DateTime.Parse("2000/01/03");
                columnOperator
                    .Setup(x => x.ToExpected(column, rowNumber, firstCell, secondCell))
                    .Returns(result);

                column.ToExpected(firstCell, secondCell, rowNumber, initializedDateTime)
                    .Should().Be(result);
            }

            [Fact]
            public void When_DateTime_values_not_equal_and_before_initialized_Should_return_Label()
            {
                var columnOperator = new Mock<IColumnOperator>();
                var column =
                    new Column(
                        "database",
                        "schema",
                        "table",
                        "column",
                        ColumnType.DateTime,
                        true,
                        1,
                        columnOperator.Object);
                int rowNumber = 1;
                var firstCell = "2000/01/01";
                var secondCell = "2000/01/02";
                var initializedDateTime = DateTime.Parse("2000/01/03");
                columnOperator
                    .Setup(x => x.ToExpected(column, rowNumber, firstCell, secondCell))
                    .Throws(new AssertionFailedException(string.Empty));

                column.ToExpected(firstCell, secondCell, rowNumber, initializedDateTime)
                    .Should().Be("TimeBeforeStart");
            }

            [Fact]
            public void When_DateTime_values_not_equal_and_after_initialized_Should_return_Label()
            {
                var columnOperator = new Mock<IColumnOperator>();
                var column =
                    new Column(
                        "database",
                        "schema",
                        "table",
                        "column",
                        ColumnType.DateTime,
                        true,
                        1,
                        columnOperator.Object);
                int rowNumber = 1;
                var firstCell = "2000/01/01";
                var secondCell = "2000/01/02";
                var initializedDateTime = DateTime.Parse("1999/12/31");
                columnOperator
                    .Setup(x => x.ToExpected(column, rowNumber, firstCell, secondCell))
                    .Throws(new AssertionFailedException(string.Empty));

                column.ToExpected(firstCell, secondCell, rowNumber, initializedDateTime)
                    .Should().Be("TimeAfterStart");
            }

            [Fact] public void When_operator_is_HostName_and_value_is_host_name_Should_return_Label()
            {
                var column =
                    new Column(
                        "database",
                        "schema",
                        "table",
                        "column",
                        ColumnType.DateTime,
                        true,
                        1,
                        new HostNameColumnOperator());
                int rowNumber = 1;
                var firstCell = Dns.GetHostName();
                var secondCell = Dns.GetHostName();
                var initializedDateTime = DateTime.Parse("1999/12/31");

                column.ToExpected(firstCell, secondCell, rowNumber, initializedDateTime)
                    .Should().Be(HostNameColumnOperator.DefaultLabel);
            }

            [Fact]
            public void When_operator_is_ignore_Should_return_Ignore()
            {
                var column =
                    new Column(
                        "database",
                        "schema",
                        "table",
                        "column",
                        ColumnType.DateTime,
                        true,
                        1,
                        new IgnoreColumnOperator());
                int rowNumber = 1;
                var firstCell = "foo";
                var secondCell = "foo";
                var initializedDateTime = DateTime.Parse("1999/12/31");

                column.ToExpected(firstCell, secondCell, rowNumber, initializedDateTime)
                    .Should().Be(IgnoreColumnOperator.DefaultLabel);
            }
        }

        public class Compare
        {
            [Fact]
            public void When_actual_equal_expected_Should_return_true()
            {
                var columnOperator = new Mock<IColumnOperator>();
                var column =
                    new Column(
                        "database",
                        "schema",
                        "table",
                        "column",
                        ColumnType.Other,
                        true,
                        1,
                        columnOperator.Object);
                var expectedCell = "foo";
                var actualCell = expectedCell;
                var timeBeforeStart = DateTime.Parse("2000/01/02");
                columnOperator
                    .Setup(x => x.Compare(expectedCell, actualCell, timeBeforeStart))
                    .Throws(new AssertionFailedException(string.Empty));

                column.Compare(expectedCell, actualCell, timeBeforeStart)
                    .Should().Be(true);
            }

            [Fact]
            public void When_values_is_not_equal_Should_return_column_operator_result()
            {
                var columnOperator = new Mock<IColumnOperator>();
                var column =
                    new Column(
                        "database",
                        "schema",
                        "table",
                        "column",
                        ColumnType.DateTime,
                        true,
                        1,
                        columnOperator.Object);
                var expectedCell = "2000/01/01";
                var actualCell = "2000/01/02";
                var result = true;
                var timeBeforeStart = DateTime.Now;
                columnOperator
                    .Setup(x => x.Compare(expectedCell, actualCell, timeBeforeStart))
                    .Returns(result);

                column.Compare(expectedCell, actualCell, timeBeforeStart)
                    .Should().Be(result);
            }

            [Fact]
            public void When_expected_is_TimeBeforeStart_and_actual_before_start_Should_return_true()
            {
                var columnOperator = new Mock<IColumnOperator>();
                var column =
                    new Column(
                        "database",
                        "schema",
                        "table",
                        "column",
                        ColumnType.DateTime,
                        true,
                        1,
                        columnOperator.Object);
                var expectedCell = "TimeBeforeStart";
                var actualCell = "2000/01/01";
                var timeBeforeStart = DateTime.Parse("2000/01/02");
                columnOperator
                    .Setup(x => x.Compare(expectedCell, actualCell, timeBeforeStart))
                    .Throws(new AssertionFailedException(string.Empty));

                column.Compare(expectedCell, actualCell, timeBeforeStart)
                    .Should().Be(true);
            }

            [Fact]
            public void When_expected_is_TimeAfterStart_and_actual_after_start_Should_return_true()
            {
                var columnOperator = new Mock<IColumnOperator>();
                var column =
                    new Column(
                        "database",
                        "schema",
                        "table",
                        "column",
                        ColumnType.DateTime,
                        true,
                        1,
                        columnOperator.Object);
                var expectedCell = "TimeAfterStart";
                var actualCell = "2000/01/02";
                var timeBeforeStart = DateTime.Parse("2000/01/01");
                columnOperator
                    .Setup(x => x.Compare(expectedCell, actualCell, timeBeforeStart))
                    .Throws(new AssertionFailedException(string.Empty));

                column.Compare(expectedCell, actualCell, timeBeforeStart)
                    .Should().Be(true);
            }

            [Fact]
            public void When_expected_is_Ignore_Should_return_true()
            {
                var columnOperator = new Mock<IColumnOperator>();
                var column =
                    new Column(
                        "database",
                        "schema",
                        "table",
                        "column",
                        ColumnType.DateTime,
                        true,
                        1,
                        columnOperator.Object);
                var expectedCell = IgnoreColumnOperator.DefaultLabel;
                var actualCell = "foo";
                var timeBeforeStart = DateTime.Parse("2000/01/01");
                columnOperator
                    .Setup(x => x.Compare(expectedCell, actualCell, timeBeforeStart))
                    .Throws(new AssertionFailedException(string.Empty));

                column.Compare(expectedCell, actualCell, timeBeforeStart)
                    .Should().Be(true);
            }

            [Fact]
            public void When_expected_is_Random_Should_return_true()
            {
                var columnOperator = new Mock<IColumnOperator>();
                var column =
                    new Column(
                        "database",
                        "schema",
                        "table",
                        "column",
                        ColumnType.DateTime,
                        true,
                        1,
                        columnOperator.Object);
                var expectedCell = RandomColumnOperator.DefaultLabel;
                var actualCell = "foo";
                var timeBeforeStart = DateTime.Parse("2000/01/01");
                columnOperator
                    .Setup(x => x.Compare(expectedCell, actualCell, timeBeforeStart))
                    .Throws(new AssertionFailedException(string.Empty));

                column.Compare(expectedCell, actualCell, timeBeforeStart)
                    .Should().Be(true);
            }

            [Fact]
            public void When_expected_is_HostName_Should_return_true()
            {
                var columnOperator = new Mock<IColumnOperator>();
                var column =
                    new Column(
                        "database",
                        "schema",
                        "table",
                        "column",
                        ColumnType.DateTime,
                        true,
                        1,
                        columnOperator.Object);
                var expectedCell = HostNameColumnOperator.DefaultLabel;
                var actualCell = Dns.GetHostName();
                var timeBeforeStart = DateTime.Parse("2000/01/01");
                columnOperator
                    .Setup(x => x.Compare(expectedCell, actualCell, timeBeforeStart))
                    .Throws(new AssertionFailedException(string.Empty));

                column.Compare(expectedCell, actualCell, timeBeforeStart)
                    .Should().Be(true);
            }
        }
    }
}
