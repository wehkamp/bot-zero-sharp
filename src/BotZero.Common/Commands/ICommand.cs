namespace BotZero.Common.Commands;

/// <summary>
/// Indicates the object is a command. A command is how the bot responds to user input.
/// It is either a direct message or a mention.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Processes a direct message or mention.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns><c>True</c> if the message was processed successful.</returns>
    Task<bool> Process(CommandContext context);

    /// <summary>
    /// Gets help lines that can be displayed.
    /// </summary>
    /// <returns>The lines.</returns>
    string[] GetHelpLines();
}

