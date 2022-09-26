using BotZero.Common.Commands.Mapping;
using BotZero.Common.Commands.Mapping.Parameters;
using FluentAssertions;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace BotZero.Common.UnitTests.Commands.Mapping.Parameters;

public class ChoiceParameterUnitTests
{
    [Theory]
    [InlineData("it is raining")]
    [InlineData("it is dripping")]
    [InlineData("it is drizzling")]
    [InlineData("it is raining cats and dogs")]
    [InlineData("it is")]
    public async Task SingleParameter(string message)
    {
        var tool = new Command("it");
        ActionContext? cmc = null;

        tool.AddAction(new CommandAction(
            "is",
            cm => { cmc = cm; return Task.CompletedTask; },
            Array.Empty<string>(),
            new IParameter[] {
                new ChoiceParameter("item1", new[]{ "raining", "dripping", "drizzling", "raining cats and dogs" }, "raining"),
            }));

        var context = new ActionContext
        {
            Message = message
        };

        var processed = await tool.Process(context);
        processed.Should().BeTrue();
        cmc.Should().NotBeNull();

        var item1 = new Regex("it is( ?)").Replace(message, "");
        var test = (string)cmc?.Values["item1"] == "raining" || (string)cmc?.Values["item1"] == item1;
        test.Should().BeTrue();
    }
}
