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
        HostNameColumnOperator Subject { get; } = new();

        public class ToExpected : HostNameColumnTypeTest
        {
            private readonly Column _column =
                new("database", "schema", "table", "column", ColumnType.Other, false, 0, new DefaultColumnOperator());

            [Fact]
            public void Where_first_equal_second_Should_return_first()
            {
                Subject.ToExpected(_column, 1, "hostname", "hostname")
                    .Should().Be("hostname");
            }

            [Fact]
            public void Where_first_equal_second_and_host_name_Should_return_label()
            {
                Subject.ToExpected(_column, 1, Dns.GetHostName(), Dns.GetHostName())
                    .Should().Be(HostNameColumnOperator.DefaultLabel);
            }

            [Fact]
            public void Where_first_not_equal_second_Should_thrown_exception()
            {
                Subject.Invoking(x => x.ToExpected(_column, 1, "foo", "bar"))
                    .ShouldThrow<DbAssertionsException>();

            }
        }

        public class Compare : HostNameColumnTypeTest
        {
            [Fact]
            public void Where_actual_equal_expected_Should_be_true()
            {
                Subject.Compare("hostname", "hostname", DateTime.Now)
                    .Should().BeTrue();
            }

            [Fact]
            public void Where_actual_is_not_expected_or_hostname_Should_be_false()
            {
                Subject.Compare("foo", $"not{Dns.GetHostName()}", DateTime.Now)
                    .Should().BeFalse();
            }

            [Fact]
            public void Where_actual_not_equal_expected_but_host_name_Should_be_true()
            {
                Subject.Compare("foo", Dns.GetHostName(), DateTime.Now)
                    .Should().BeTrue();
            }
        }
    }
}
