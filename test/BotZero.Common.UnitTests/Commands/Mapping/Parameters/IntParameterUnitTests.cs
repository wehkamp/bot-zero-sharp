using BotZero.Common.Commands.Mapping;
using BotZero.Common.Commands.Mapping.Parameters;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BotZero.Common.UnitTests.Commands.Mapping.Parameters;

public class IntParameterUnitTests
{
    [Fact]
    public async Task MultipleParameters()
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
                new IntParameter("item2")
            }));

        var context = new ActionContext
        {
            Message = ""
        };

        context.Message = "test now 1 2";
        await tool.Process(context);
        lastInvokedContext.Should().NotBeNull();
        lastInvokedContext?.Values["item1"].Should().Be(1);
        lastInvokedContext?.Values["item2"].Should().Be(2);
        lastInvokedContext = null;

        context.Message = "test now 11 22";
        await tool.Process(context);
        lastInvokedContext.Should().NotBeNull();
        lastInvokedContext?.Values["item1"].Should().Be(11);
        lastInvokedContext?.Values["item2"].Should().Be(22);
        lastInvokedContext = null;

        context.Message = "test now 1 22";
        await tool.Process(context);
        lastInvokedContext.Should().NotBeNull();
        lastInvokedContext?.Values["item1"].Should().Be(1);
        lastInvokedContext?.Values["item2"].Should().Be(22);
        lastInvokedContext = null;

        context.Message = "test now 11 2";
        await tool.Process(context);
        lastInvokedContext.Should().NotBeNull();
        lastInvokedContext?.Values["item1"].Should().Be(11);
        lastInvokedContext?.Values["item2"].Should().Be(2);
        lastInvokedContext = null;

        context.Message = "test now -11 -22";
        await tool.Process(context);
        lastInvokedContext.Should().NotBeNull();
        lastInvokedContext?.Values["item1"].Should().Be(-11);
        lastInvokedContext?.Values["item2"].Should().Be(-22);
        lastInvokedContext = null;
    }

    [Fact]
    public async Task SingleParameter()
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
                new IntParameter("item1")
            }));

        var context = new ActionContext
        {
            Message = ""
        };

        context.Message = "test now 1";
        await tool.Process(context);
        lastInvokedContext.Should().NotBeNull();
        lastInvokedContext?.Values["item1"].Should().Be(1);
        lastInvokedContext = null;

        context.Message = "test now 11";
        await tool.Process(context);
        lastInvokedContext.Should().NotBeNull();
        lastInvokedContext?.Values["item1"].Should().Be(11);
        lastInvokedContext = null;

        context.Message = "test now -11";
        await tool.Process(context);
        lastInvokedContext.Should().NotBeNull();
        lastInvokedContext?.Values["item1"].Should().Be(-11);
        lastInvokedContext = null;
    }
}
