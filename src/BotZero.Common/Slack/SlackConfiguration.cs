using System.ComponentModel.DataAnnotations;

namespace BotZero.Common.Slack;

public class SlackConfiguration
{
    [Required]
    public string? Token { get; set; }

    [Required]
    public string? AppToken { get; set; }

    public int MaxConcurrency { get; set; } = 32;
}
