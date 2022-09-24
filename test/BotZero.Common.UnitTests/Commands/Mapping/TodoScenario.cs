using BotZero.Common.Commands.Mapping;
using BotZero.Common.Commands.Mapping.Parameters;
using FluentAssertions;
using System.Threading.Tasks;
using Xunit;

namespace BotZero.Common.UnitTests.Commands.Mapping;

public class TodoScenario
{
    class TodoTestTool
    {
        public string? lastInvoked = null;
        public ActionContext? lastInvokedContext = null;
        public Command tool;

        public TodoTestTool()
        {

            Task Add(ActionContext cm)
            {
                lastInvoked = "add";
                lastInvokedContext = cm;
                return Task.CompletedTask;
            }

            Task Remove(ActionContext cm)
            {
                lastInvoked = "remove";
                lastInvokedContext = cm;
                return Task.CompletedTask;
            }

            Task List(ActionContext cm)
            {
                lastInvoked = "list";
                lastInvokedContext = cm;
                return Task.CompletedTask;
            }

            tool = new Command("todo");
            tool.AddAction("add", Add, new string[] { "" }, new IParameter[] { new RestParameter("item") });
            tool.AddAction("remove", Remove, new string[] { "rm", "del" }, new IParameter[] { new RestParameter("item") });
            tool.AddAction("list", List, "", "lst", "ls");
        }
    }

    [Fact]
    public async Task AddUsingNamedCommand()
    {
        var test = new TodoTestTool();
        var context = new ActionContext
        {
            Message = "todo add boter"
        };

        await test.tool.Process(context);
        test.lastInvoked.Should().Be("add", "named add command");
        test.lastInvokedContext?.Values["item"].Should().Be("boter");
    }


    [Fact]
    public async Task AddUsingDefaultAlias()
    {
        var test = new TodoTestTool();
        var context = new ActionContext
        {
            Message = "todo kaas"
        };
        await test.tool.Process(context);
        test.lastInvoked.Should().Be("add", "empty alias for add");
        test.lastInvokedContext?.Values["item"].Should().Be("kaas");
    }

    [Fact]
    public async Task ListUsingNamedCommand()
    {
        var test = new TodoTestTool();
        var context = new ActionContext
        {
            Message = "todo list"
        };
        await test.tool.Process(context);
        test.lastInvoked.Should().Be("list", "name list command");
    }

    [Fact]
    public async Task ListUsingDefaultAlias()
    {
        var test = new TodoTestTool();
        var context = new ActionContext
        {
            Message = "todo"
        };
        await test.tool.Process(context);
        test.lastInvoked.Should().Be("list", "empty alias for list");
    }

    [Fact]
    public async Task ListUsingAlias()
    {
        var test = new TodoTestTool();
        var context = new ActionContext
        {
            Message = "todo ls"
        };
        await test.tool.Process(context);
        test.lastInvoked.Should().Be("list", "alias for list");
    }
    [Fact]
    public async Task RemoveUsingNamedCommand()
    {
        var test = new TodoTestTool();
        var context = new ActionContext
        {
            Message = "todo remove eieren"
        };
        await test.tool.Process(context);
        test.lastInvoked.Should().Be("remove", "named remove command");
        test.lastInvokedContext?.Values["item"].Should().Be("eieren");
    }

    [Fact]
    public async Task AddUsingNamedCommandWithOtherNamedCommandInParameter()
    {
        var test = new TodoTestTool();
        var context = new ActionContext
        {
            Message = "todo add remove eieren"
        };
        await test.tool.Process(context);
        test.lastInvoked.Should().Be("add", "named remove command");
        test.lastInvokedContext?.Values["item"].Should().Be("remove eieren");
    }

    [Fact]
    public async Task AddWithoutParameterShouldFail()
    {
        var test = new TodoTestTool();
        var context = new ActionContext
        {
            Message = "todo add"
        };
        await test.tool.Process(context);
        test.lastInvoked.Should().BeNull("Add without parameters should not be invoked.");
        test.lastInvokedContext.Should().BeNull("Add without parameters should not be invoked.");
    }

    [Fact]
    public async Task RemoveWithoutParameterShouldFail()
    {
        var test = new TodoTestTool();
        var context = new ActionContext
        {
            Message = "todo remove"
        };
        await test.tool.Process(context);
        test.lastInvoked.Should().BeNull("Remove without parameters should not be invoked.");
        test.lastInvokedContext.Should().BeNull("Remove without parameters should not be invoked.");
    }

    [Fact]
    public async Task RemoveWithAliasAndWithoutParameterShouldFail()
    {
        var test = new TodoTestTool();
        var context = new ActionContext
        {
            Message = "todo rm"
        };
        await test.tool.Process(context);
        test.lastInvoked.Should().BeNull();
        test.lastInvoked.Should().BeNull("Remove alias without parameters should not be invoked.");
        test.lastInvokedContext.Should().BeNull("Remove alias without parameters should not be invoked.");
    }
}
