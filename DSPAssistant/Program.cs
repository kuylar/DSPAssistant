using System.Reflection;
using DSharpPlus;
using DSharpPlus.ButtonCommands;
using DSharpPlus.ButtonCommands.Extensions;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSPAssistant;
using DSPAssistant.ButtonCommands;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Extensions.Logging;

Log.Logger = new LoggerConfiguration()
	.WriteTo.Console()
	.CreateLogger();

DiscordClient client = new(new DiscordConfiguration
{
	Token = Environment.GetEnvironmentVariable("DISCORD_TOKEN") ??
	        throw new Exception("Please set the DISCORD_TOKEN environment variable"),
	LoggerFactory = new SerilogLoggerFactory(Log.Logger)
});

Utils.SetConnectionString(
	Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? throw new Exception("Please set the POSTGRES_HOST environment variable"),
	Environment.GetEnvironmentVariable("POSTGRES_USERNAME") ?? throw new Exception("Please set the POSTGRES_USERNAME environment variable"),
	Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? throw new Exception("Please set the POSTGRES_PASSWORD environment variable"),
	Environment.GetEnvironmentVariable("POSTGRES_DB") ?? throw new Exception("Please set the POSTGRES_DB environment variable")
);
DatabaseContext database = Utils.CreateDatabaseContext();

Log.Information("Running EFCore migrations...");
database.Database.EnsureCreated();
Log.Information("EFCore initialization done!");

ButtonCommandsExtension bce = client.UseButtonCommands(new ButtonCommandsConfiguration
{
	ArgumentSeparator = "␞",
	Prefix = "␁"
});
bce.RegisterButtons<VoteCommandsModule>();
bce.RegisterButtons<ShowOtherResultModule>();
Utils.SetButtonCommands(bce);

client.UseSlashCommands(new SlashCommandsConfiguration())
	.RegisterCommands(Assembly.GetExecutingAssembly(), 917263628846108683);

client.ThreadCreated += EventHandlers.OnThreadCreated;
client.Ready += EventHandlers.OnReady;
client.MessageCreated += EventHandlers.OnMessageCreated;

Log.Information("Logging into Discord...");
await client.ConnectAsync();
await Task.Delay(-1);