using BotZero.Common.Commands.Mapping;
using BotZero.Common.Commands.Mapping.Parameters;
using FluentAssertions;
using System.Threading.Tasks;
using Xunit;

namespace BotZero.Common.UnitTests.Commands.Mapping;

public class CommandUnitTests
{
    [Fact]
    public async Task PartialMatchTest()
    {
        var tool = new Command("todo");

        tool.AddAction(
            "rem",
            cm => Task.CompletedTask,
            new[] { "del" },
            new[] { new RestParameter("item") });

        var context = new ActionContext
        {
            Message = ""
        };

        var executed = false;

        context.Message = "todo rem item";
        executed = await tool.Process(context);
        executed.Should().BeTrue("rem is the command name.");

        context.Message = "todo remove item";
        executed = await tool.Process(context);
        executed.Should().BeFalse("remove is not the command name or an alias.");

        context.Message = "todo del item";
        executed = await tool.Process(context);
        executed.Should().BeTrue("del is an alias.");

        context.Message = "todo delete item";
        executed = await tool.Process(context);
        executed.Should().BeFalse("delete is not the command name or an alias.");
    }
}
