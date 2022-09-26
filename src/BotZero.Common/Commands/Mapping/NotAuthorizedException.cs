namespace BotZero.Common.Commands.Mapping
{
    [Serializable]
    public class NotAuthorizedException : Exception
    {
        public NotAuthorizedException(string commandName, string actionName) : base()
        {
            CommandName = commandName;
            ActionName = actionName;
        }

        public string CommandName { get; }

        public string ActionName { get; }
    }
}