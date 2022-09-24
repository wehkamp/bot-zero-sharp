using BotZero.Common.Commands.Mapping;
using BotZero.Common.Commands.Mapping.Attributes;
using BotZero.Common.Commands.Mapping.Parameters;
using FluentAssertions;
using Slack.NetStandard;
using System.Threading.Tasks;
using Xunit;

namespace BotZero.Common.UnitTests.Commands.Mapping;

public class TodoWithAutoCommandMapperScenario
{
    [Command("todo")]
    [Help(
        "todo - lists the todos.",
        "todo {item} - adds a new item to the todo list",
        "todo remove {item} - removes all the items that (partially) match the item"
    )]
    class TodoWithAutoMapper : CommandMapper
    {
        public string? lastInvoked = null;
        public string? lastItem = null;

        public TodoWithAutoMapper() : base(new SlackWebApiClient("FAKE_TOKEN"))
        {
        }

        [Action("")]
        protected Task Add(
            [RestParameter]
            string item)
        {
            lastInvoked = "add";
            lastItem = item;
            return Task.CompletedTask;
        }

        [Action("rm", "rem", "del", "delete")]
        protected Task Remove(string item)
        {
            lastInvoked = "remove";
            lastItem = item;
            return Task.CompletedTask;
        }

        [Action("", "ls", "lst")]
        protected Task List()
        {
            lastInvoked = "list";
            return Task.CompletedTask;
        }

    }

    [Fact]
    public async Task AddUsingNamedCommand()
    {
        var test = new TodoWithAutoMapper();
        var context = new ActionContext
        {
            Message = "todo add boter"
        };

        await test.Process(context);
        test.lastInvoked.Should().Be("add", "named add command");
        test.lastItem.Should().Be("boter");
    }


    [Fact]
    public async Task AddUsingDefaultAlias()
    {
        var test = new TodoWithAutoMapper();
        var context = new ActionContext
        {
            Message = "todo kaas"
        };
        await test.Process(context);
        test.lastInvoked.Should().Be("add", "empty alias for add");
        test.lastItem.Should().Be("kaas");
    }

    [Fact]
    public async Task ListUsingNamedCommand()
    {
        var test = new TodoWithAutoMapper();
        var context = new ActionContext
        {
            Message = "todo list"
        };
        await test.Process(context);
        test.lastInvoked.Should().Be("list", "name list command");
    }

    [Fact]
    public async Task ListUsingDefaultAlias()
    {
        var test = new TodoWithAutoMapper();
        var context = new ActionContext
        {
            Message = "todo"
        };
        await test.Process(context);
        test.lastInvoked.Should().Be("list", "empty alias for list");
    }

    [Fact]
    public async Task ListUsingAlias()
    {
        var test = new TodoWithAutoMapper();
        var context = new ActionContext
        {
            Message = "todo ls"
        };
        await test.Process(context);
        test.lastInvoked.Should().Be("list", "alias for list");
    }

    [Fact]
    public async Task RemoveUsingNamedCommand()
    {
        var test = new TodoWithAutoMapper();
        var context = new ActionContext
        {
            Message = "todo remove eieren"
        };
        await test.Process(context);
        test.lastInvoked.Should().Be("remove", "named remove command");
        test.lastItem.Should().Be("eieren");
    }

    [Fact]
    public async Task AddUsingNamedCommandWithOtherNamedCommandInParameter()
    {
        var test = new TodoWithAutoMapper();
        var context = new ActionContext
        {
            Message = "todo add remove eieren"
        };
        await test.Process(context);
        test.lastInvoked.Should().Be("add", "named remove command");
        test.lastItem.Should().Be("remove eieren");
    }
}
