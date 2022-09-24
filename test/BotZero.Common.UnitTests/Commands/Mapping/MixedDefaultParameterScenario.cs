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
    public class MixedParameterScenario
    {
        class TestCommand : CommandMapper
        {
            public string? lastInvoked = null;

            public TestCommand() : base(new SlackWebApiClient("FAKE_TOKEN"))
            {
            }

            [Action]
            protected Task Now(
               int item1,
               string item2,
               [IpParameter] string item3,
               [LabelParameter("feels like")] string item4,
               [ChoiceParameter("home", "school")] string item5,
               [RegexParameter(".{3}")] string item6,
               [RestParameter] string item7
            )
            {
                lastInvoked = "now";
                return Task.CompletedTask;
            }
        }


        [Fact]
        public async Task Test()
        {
            var tool = new TestCommand();
            var context = new ActionContext
            {
                Message = "test now 42 1337 1.0.0.127 feels like home and happiness!!"
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