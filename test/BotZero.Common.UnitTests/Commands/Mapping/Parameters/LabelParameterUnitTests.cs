using BotZero.Common.Commands.Mapping;
using BotZero.Common.Commands.Mapping.Parameters;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BotZero.Common.UnitTests.Commands.Mapping.Parameters;

public class LabelParameterUnitTests
{
    [Theory]
    [InlineData("label", "label")]
    [InlineData("label", "la bel")]
    [InlineData("la bel", "label")]
    [InlineData("la bel", "la bel")]
    [InlineData("^^^", "$$$")]
    public async Task MultipleParameters(string item1, string item2)
    {
        ActionContext? lastInvokedContext = null;

        Task Add(ActionContext cm)
        {
            lastInvokedContext = cm;
            return Task.CompletedTask;
        }

        var tool = new Command("test");

        tool.AddAction(new CommandAction(
            "now",
            Add,
            Array.Empty<string>(),
            new IParameter[] {
                new LabelParameter("item1", item1),
                new LabelParameter("item2", item2)
            }));

        var context = new ActionContext
        {
            Message = "test now " + item1 + " " + item2
        };

        await tool.Process(context);
        lastInvokedContext.Should().NotBeNull();
        lastInvokedContext?.Values["item1"].Should().Be(item1);
        lastInvokedContext?.Values["item2"].Should().Be(item2);
    }
}
