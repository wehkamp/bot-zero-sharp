using BotZero.Common.Commands.Mapping;
using BotZero.Common.Commands.Mapping.Parameters;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BotZero.Common.UnitTests.Commands.Mapping;

public class CounterScenario
{
    [Theory()]
    [InlineData("test count from 7 to 9")]
    [InlineData("test count 7 to 9")]
    [InlineData("test count 7 9")]
    public async Task Count(string message)
    {
        ActionContext? lastInvokedContext = null;

        Task Count(ActionContext cm)
        {
            lastInvokedContext = cm;
            return Task.CompletedTask;
        }

        var tool = new Command("test");

        tool.AddAction(new CommandAction(
            "count",
            Count,
            Array.Empty<string>(),
            new IParameter[] {
                new LabelParameter("flabel", "from", true),
                new IntParameter("from"),
                new LabelParameter("tlabel", "to", true),
                new IntParameter("to")
            }));

        var context = new ActionContext
        {
            Message = message
        };

        await tool.Process(context);
        lastInvokedContext.Should().NotBeNull();
        lastInvokedContext?.Values["flabel"].Should().Be("from");
        lastInvokedContext?.Values["from"].Should().Be(7);
        lastInvokedContext?.Values["tlabel"].Should().Be("to");
        lastInvokedContext?.Values["to"].Should().Be(9);
    }
}
