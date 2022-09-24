using BotZero.Common.Commands.Mapping;
using BotZero.Common.Commands.Mapping.Attributes;
using BotZero.Common.Commands.Mapping.Parameters;
using FluentAssertions;
using Slack.NetStandard;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BotZero.Common.UnitTests.Commands.Mapping
{
    public class MixedDefaultParameterScenario
    {
        class TestCommand : CommandMapper
        {
            public string? lastInvoked = null;

            public TestCommand() : base(new SlackWebApiClient("FAKE_TOKEN"))
            {
            }

            [Action]
            protected Task Now(
                [IntParameter(42)] int item1,
                [StringParameter("1337")] string item2,
                [IpParameter] string item3,
                [LabelParameter("feels like", true)] string item4,
                [ChoiceParameter(new[] { "home", "school" }, "home")] string item5,
                [RegexParameter(".{3}")] string item6,
                [RestParameter] string item7
            )
            {
                lastInvoked = "now";
                return Task.CompletedTask;
            }
        }

        [Theory]
        [InlineData("test now 42 1337 1.0.0.127 feels like home and happiness!!")]
        [InlineData("test now 42 1.0.0.127 feels like home and happiness!!")]
        [InlineData("test now \"1337\" 1.0.0.127 feels like home and happiness!!")]
        [InlineData("test now 1.0.0.127 feels like home and happiness!!")]
        [InlineData("test now 1.0.0.127 home and happiness!!")]
        [InlineData("test now 1.0.0.127 and happiness!!")]
        public async Task Test(string message)
        {
            var tool = new TestCommand();
            var context = new ActionContext
            {
                Message = message
            };

            await tool.Process(context);

            tool.Context.Should().NotBeNull();
            tool.Context.Values["item1"].Should().Be(42);
            tool.Context.Values["item2"].Should().Be("1337");
            tool.Context.Values["item3"].Should().Be("1.0.0.127");
            tool.Context.Values["item4"].Should().Be("feels like");
            tool.Context.Values["item5"].Should().Be("home");
            tool.Context.Values["item6"].Should().Be("and");
            tool.Context.Values["item7"].Should().Be("happiness!!");
        }
    }
}