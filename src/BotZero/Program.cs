using BotZero.Common;
using BotZero.Common.Slack;
using BotZero.Common.Templating;

var builder = WebApplication.CreateBuilder(args);

// init Slack config
builder.Services.ConfigureAndValidate<SlackConfiguration>("Slack", builder.Configuration);

// init slack bot
builder.Services.AddChatBot();

// init template generator
builder.Services.AddTransient<IJsonTemplateGenerator, JsonTemplateGenerator>();

// other services
builder.Services
    .AddHttpClient()
    .AddMemoryCache();

var app = builder.Build();

app.MapGet("/status", () => new { status = "OK" });
app.MapGet("/health/alive", () => new { status = "OK" });
app.MapGet("/health/ready", () => new { status = "OK" });

app.Run();

