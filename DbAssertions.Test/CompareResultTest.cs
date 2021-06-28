using FluentAssertions;
using Xunit;

namespace DbAssertions.Test
{
    public class CompareResultTest
    {
        public class ResultsWillBeManaged
        {
            [Fact]
            public void When_error_not_registered()
            {
                CompareResult compareResult = new();
                compareResult.HasMismatched.Should().BeFalse();
                compareResult.MismatchedMessages.Should().BeEmpty();
            }

            [Fact]
            public void When_registered()
            {
                CompareResult compareResult = new();

                var message1 = "message1";
                var message2 = "message2";
                compareResult.AddMismatchedMessage(message1);
                compareResult.AddMismatchedMessage(message2);


                compareResult.HasMismatched
                    .Should().BeTrue();
                compareResult.MismatchedMessages
                    .Should().Equal(message1, message2);

            }
        }
    }
}