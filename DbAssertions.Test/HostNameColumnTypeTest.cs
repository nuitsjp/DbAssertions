using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using System.Net;

namespace DbAssertions.Test
{
    public class HostNameColumnTypeTest
    {
        HostNameColumnType Subject { get; } = new();

        public class Compare : HostNameColumnTypeTest
        {
            [Fact]
            public void When_value_is_equal_Should_be_true()
            {
                Subject.Compare("foo", "foo", DateTime.Now)
                    .Should().BeTrue();
            }

            [Fact]
            public void When_value_is_not_equal_Should_be_false()
            {
                Subject.Compare("foo", "var", DateTime.Now)
                    .Should().BeFalse();
            }


            [Fact]
            public void When_expected_is_label_and_actual_is_host_name_Should_be_true()
            {
                Subject.Compare("HostName", Dns.GetHostName(), DateTime.Now)
                    .Should().BeTrue();
            }


            [Fact]
            public void When_expected_is_label_and_actual_is_not_host_name_Should_be_true()
            {
                Subject.Compare("HostName", $"not_{Dns.GetHostName()}", DateTime.Now)
                    .Should().BeFalse();
            }
        }

        public class ToExpected : HostNameColumnTypeTest
        {
            private readonly Column _column =
                new ("database", "schema", "table", "column", ColumnType.Other, false, 0);

            [Fact]
            public void When_value_is_equal_and_not_matches_host_name_Should_return_value()
            {
                Subject.ToExpected(_column, 1, "foo", "foo")
                    .Should().Be("foo");
            }

            [Fact]
            public void When_value_is_equal_and_matches_host_name_Should_return_host_name_label()
            {
                Subject.ToExpected(_column, 1, Dns.GetHostName(), Dns.GetHostName())
                    .Should().Be("HostName");
            }

            [Fact]
            public void When_value_is_not_equal_Should_throw_exception()
            {
                Subject.Invoking(x => x.ToExpected(_column, 1, "foo", "var"))
                    .ShouldThrow<DbAssertionsException>();
            }
        }
    }
}
