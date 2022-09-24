using BotZero.Common.Commands.Mapping;
using BotZero.Common.Commands.Mapping.Parameters;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BotZero.Common.UnitTests.Commands.Mapping.Parameters;

public class StringParameterUnitTests
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

        tool.AddAction(
            "now",
            Add,
            Array.Empty<string>(),
            new IParameter[] {
                new StringParameter("item1"),
                new StringParameter("item2")
            });

        var context = new ActionContext
        {
            Message = ""
        };

        context.Message = "test now item1 item2";
        await tool.Process(context);
        lastInvokedContext.Should().NotBeNull();
        lastInvokedContext?.Values["item1"].Should().Be("item1");
        lastInvokedContext?.Values["item2"].Should().Be("item2");
        lastInvokedContext = null;

        context.Message = "test now \"item 1\" \"item 2\"";
        await tool.Process(context);
        lastInvokedContext.Should().NotBeNull();
        lastInvokedContext?.Values["item1"].Should().Be("item 1");
        lastInvokedContext?.Values["item2"].Should().Be("item 2");
        lastInvokedContext = null;

        context.Message = "test now item1 \"item 2\"";
        await tool.Process(context);
        lastInvokedContext.Should().NotBeNull();
        lastInvokedContext?.Values["item1"].Should().Be("item1");
        lastInvokedContext?.Values["item2"].Should().Be("item 2");
        lastInvokedContext = null;

        context.Message = "test now \"item 1\" item2";
        await tool.Process(context);
        lastInvokedContext.Should().NotBeNull();
        lastInvokedContext?.Values["item1"].Should().Be("item 1");
        lastInvokedContext?.Values["item2"].Should().Be("item2");
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

        tool.AddAction(
            "now",
            Add,
            Array.Empty<string>(),
            new IParameter[] {
                new StringParameter("item1")
            });

        var context = new ActionContext
        {
            Message = ""
        };

        context.Message = "test now item1";
        await tool.Process(context);
        lastInvokedContext.Should().NotBeNull();
        lastInvokedContext?.Values["item1"].Should().Be("item1");
        lastInvokedContext = null;

        context.Message = "test now \"item 1\"";
        await tool.Process(context);
        lastInvokedContext.Should().NotBeNull();
        lastInvokedContext?.Values["item1"].Should().Be("item 1");
        lastInvokedContext = null;
    }

    [Fact]
    public async Task SingleParameterWithDefaultValue()
    {
        ActionContext? lastInvokedContext = null;

        Task Add(ActionContext cm)
        {
            lastInvokedContext = cm;
            return Task.CompletedTask;
        }

        var tool = new Command("test");

        tool.AddAction(
            "now",
            Add,
            Array.Empty<string>(),
            new IParameter[] {
                new StringParameter("item1", "hi")
            });

        var context = new ActionContext
        {
            Message = ""
        };

        context.Message = "test now";
        await tool.Process(context);
        lastInvokedContext.Should().NotBeNull();
        lastInvokedContext?.Values["item1"].Should().Be("hi");
        lastInvokedContext = null;
    }

    [Fact]
    public async Task LastParameterWithDefaultValue()
    {
        ActionContext? lastInvokedContext = null;

        Task Add(ActionContext cm)
        {
            lastInvokedContext = cm;
            return Task.CompletedTask;
        }

        var tool = new Command("test");

        tool.AddAction(
            "now",
            Add,
            Array.Empty<string>(),
            new IParameter[] {
                new StringParameter("item1"),
                new StringParameter("item2", "hi")
            });

        var context = new ActionContext
        {
            Message = ""
        };

        context.Message = "test now item1";
        await tool.Process(context);
        lastInvokedContext.Should().NotBeNull();
        lastInvokedContext?.Values["item1"].Should().Be("item1");
        lastInvokedContext?.Values["item2"].Should().Be("hi");
        lastInvokedContext = null;
    }

    [Fact]
    public async Task TwoOptionalParameters()
    {
        ActionContext? lastInvokedContext = null;

        Task Add(ActionContext cm)
        {
            lastInvokedContext = cm;
            return Task.CompletedTask;
        }

        var tool = new Command("test");

        tool.AddAction(
            "now",
            Add,
            Array.Empty<string>(),
            new IParameter[] {
                new StringParameter("item1", "h1"),
                new StringParameter("item2", "h2")
            });

        var context = new ActionContext
        {
            Message = ""
        };

        context.Message = "test now";
        await tool.Process(context);
        lastInvokedContext.Should().NotBeNull();
        lastInvokedContext?.Values["item1"].Should().Be("h1");
        lastInvokedContext?.Values["item2"].Should().Be("h2");
        lastInvokedContext = null;

        context.Message = "test now i1";
        await tool.Process(context);
        lastInvokedContext.Should().NotBeNull();
        lastInvokedContext?.Values["item1"].Should().Be("i1");
        lastInvokedContext?.Values["item2"].Should().Be("h2");
        lastInvokedContext = null;
    }
}
