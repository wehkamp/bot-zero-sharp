using BotZero.Common.Commands.Mapping;
using BotZero.Common.Commands.Mapping.Attributes;
using BotZero.Common.Commands.Mapping.Parameters;
using Slack.NetStandard;

namespace BotZero.Commands;

[Help(
    "todo - shows the todo list.",
    "todo `{item}` - adds a new item to the list.",
    "todo remove `{item}` - removes items that partially match the input."
)]
public class Todo : CommandMapper
{
    private static readonly List<string> _todos = new();

    public Todo(SlackWebApiClient client) : base(client)
    {
    }

    [Action("")]
    protected async Task Add([RestParameter]string item)
    {
        _todos.Add(item);
        await Reply($"Added _{item}_ to the list.");
    }

    [Action("rm", "del", "rem", "delete")]
    protected async Task Remove([RestParameter]string item)
    {
        var length = _todos.Count;
        _todos
            .Where(x => x.Contains(item, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .ForEach(x => _todos.Remove(x));

        var i = length - _todos.Count;
        if (i == 1)
        {
            await Reply("1 item was removed.");
        }
        else
        {
            await Reply($"{i} items were removed.");
        }
    }

    [Action("", "ls", "dir", "ls")]
    protected async Task List()
    {
        if(_todos.Count == 0)
        {
            await Reply("The list is empty.");
            return;
        }

        var i = 0;
        var str = "The following items are on the list:";
        _todos.ForEach(t => str += $"\n{++i}. {t}");

        await Reply(str);
    }
}
