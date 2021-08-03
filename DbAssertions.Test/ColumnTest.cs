using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using Moq;
using Xunit;

namespace DbAssertions.Test
{
    public class ColumnTest
    {
        public class ToExpected
        {
            [Fact]
            public void Should_return_column_operator_result()
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
                columnOperator
                    .Setup(x => x.ToExpected(column, rowNumber, firstCell, secondCell))
                    .Returns(result);

                column.ToExpected(firstCell, secondCell, rowNumber)
                    .Should().Be(result);
            }
        }

        public class Compare
        {
            [Fact]
            public void Should_return_column_operator_result()
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
                var actualCell = "bar";
                var result = true;
                var timeBeforeStart = DateTime.Now;
                columnOperator
                    .Setup(x => x.Compare(expectedCell, actualCell, timeBeforeStart))
                    .Returns(result);

                column.Compare(expectedCell, actualCell, timeBeforeStart)
                    .Should().Be(result);
            }
        }
    }
}
