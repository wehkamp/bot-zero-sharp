using BotZero.Common.Commands;
using BotZero.Common.Commands.Mapping;
using BotZero.Common.Commands.Mapping.Attributes;
using FluentAssertions;
using Slack.NetStandard;
using System.Threading.Tasks;
using Xunit;

namespace BotZero.Common.UnitTests.Commands.Mapping
{
    public class CommandMapperUnitTests
    {
        class TestCommand : CommandMapper
        {
            public string? item = null;
            public bool executed = false;

            public TestCommand() : base(new SlackWebApiClient("FAKE_TOKEN"))
            {
            }

            protected void Callback(string item)
            {
                executed = true;
                this.item = item;
            }
        }

        [Fact]
        public async Task DefaultCallback()
        {
            var command = new TestCommand();
            var context = new CommandContext
            {
                Message = "test 42"
            };
            var processed = await command.Process(context);

            processed.Should().BeTrue();
            command.executed.Should().BeTrue();
            command.item.Should().Be("42");
        }

        [Command("test")]
        class TestCommandWithDefaultParameter : CommandMapper
        {
            public string? item = null;
            public bool executed = false;

            public TestCommandWithDefaultParameter() : base(new SlackWebApiClient("FAKE_TOKEN"))
            {
            }

            protected void Callback(string item = "42")
            {
                executed = true;
                this.item = item;
            }
        }

        [Theory]
        [InlineData("test")]
        [InlineData("TEST")]
        [InlineData("test 42")]
        [InlineData("TEST 42")]
        public async Task DefaultCallbackWithOptionalParameter(string msg)
        {
            var command = new TestCommandWithDefaultParameter();
            var context = new CommandContext
            {
                Message = msg
            };
            var processed = await command.Process(context);

            processed.Should().BeTrue();
            command.executed.Should().BeTrue();
            command.item.Should().Be("42");
        }

        [Command("test")]
        class TestCommandWithTask : CommandMapper
        {
            public string? item = null;
            public bool executed = false;

            public TestCommandWithTask() : base(new SlackWebApiClient("FAKE_TOKEN"))
            {
            }

            protected async Task Callback(string item)
            {
                await Task.Delay(10);
                executed = true;
                this.item = item;
            }
        }

        [Fact]
        public async Task DefaultCallbackWithTask()
        {
            var command = new TestCommandWithTask();
            var context = new CommandContext
            {
                Message = "test 42"
            };
            var processed = await command.Process(context);

            processed.Should().BeTrue();
            command.executed.Should().BeTrue();
            command.item.Should().Be("42");
        }

        [Command("test")]
        class TestCommandWithTaskAndDefaultParameter : CommandMapper
        {
            public string? item = null;
            public bool executed = false;

            public TestCommandWithTaskAndDefaultParameter() : base(new SlackWebApiClient("FAKE_TOKEN"))
            {
            }

            protected async Task Callback(string item = "42")
            {
                await Task.Delay(10);
                executed = true;
                this.item = item;
            }
        }


        [Theory]
        [InlineData("test")]
        [InlineData("TEST")]
        [InlineData("test 42")]
        [InlineData("TEST 42")]
        public async Task DefaultCallbackTaskWithOptionalParameter(string msg)
        {
            var command = new TestCommandWithTaskAndDefaultParameter();
            var context = new CommandContext
            {
                Message = msg
            };
            var processed = await command.Process(context);

            processed.Should().BeTrue();
            command.executed.Should().BeTrue();
            command.item.Should().Be("42");
        }
    }
}
