using BotZero.Common.Commands.Mapping;
using BotZero.Common.Commands.Mapping.Parameters;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BotZero.Common.UnitTests.Commands.Mapping.Parameters;

public class MixedParameterUnitTests
{
    [Fact]
    public async Task Test()
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
                new IntParameter("item1"),
                new StringParameter("item2"),
                new IpParameter("item3"),
                new LabelParameter("item4","feels like"),
                new ChoiceParameter("item5", new []{"home","school"}),
                new RestParameter("item6")
            }));

        var context = new ActionContext
        {
            Message = "test now 42 1337 1.0.0.127 feels like home and happiness!!"
        };

        await tool.Process(context);

        lastInvokedContext.Should().NotBeNull();
        lastInvokedContext?.Values["item1"].Should().Be(42);
        lastInvokedContext?.Values["item2"].Should().Be("1337");
        lastInvokedContext?.Values["item3"].Should().Be("1.0.0.127");
        lastInvokedContext?.Values["item4"].Should().Be("feels like");
        lastInvokedContext?.Values["item5"].Should().Be("home");
        lastInvokedContext?.Values["item6"].Should().Be("and happiness!!");
    }
}
