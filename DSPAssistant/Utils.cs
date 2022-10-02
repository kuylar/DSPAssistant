using DSharpPlus;
using DSharpPlus.ButtonCommands;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Serilog;

namespace DSPAssistant;

public static class Utils
{
	private static string _connectionString;
	private static ButtonCommandsExtension _bce;

	public static void SetConnectionString(string host, string username, string password, string database)
	{
		_connectionString = $"Server={host};Port=5432;Database={database};User Id={username};Password={password};";
		Log.Information("Using connection string {ConnStr}",
			_connectionString.Replace("Password=" + password, "Password=<PASSWORD REMOVED>"));
	}

	public static string GetConnectionString() => _connectionString;

	public static void SetButtonCommands(ButtonCommandsExtension e) => _bce = e;

	public static string GetButtonId(string commandName, params object[] arguments) =>
		_bce.BuildButtonId(commandName, arguments);

	// probably a bad idea idk im not being paid enough for this
	public static DatabaseContext CreateDatabaseContext() => new();

	public static DiscordInteractionResponseBuilder BuildCreateSuggestionModal(Suggestion? suggestion = null)
	{
		DiscordInteractionResponseBuilder modal = new();

		if (suggestion is not null)
			modal
				.WithCustomId($"editSuggestion {suggestion.Id}")
				.WithTitle($"Edit {suggestion.Synopsis}");
		else
			modal
				.WithCustomId("newSuggestion")
				.WithTitle("Create a new suggestion")
				.AddComponents(new TextInputComponent(
					"ID",
					"id",
					"A unique ID for this suggestion"));

		modal
			.AddComponents(new TextInputComponent(
				"Title",
				"title",
				"Title of this suggestion",
				suggestion?.Synopsis))
			.AddComponents(
				new TextInputComponent(
					"Description",
					"description",
					"Detailed description on what's wrong and what the user can do to fix their issue",
					suggestion?.Description,
					true,
					TextInputStyle.Paragraph))
			.AddComponents(
				new TextInputComponent(
					"Code Example",
					"code",
					"// Remember: no spoonfeeding\n" +
					"// Everything here will be wrapped in a ```cs codeblock",
					suggestion?.CodeExample,
					false,
					TextInputStyle.Paragraph))
			.AddComponents(new TextInputComponent(
				"Image URL",
				"image",
				"A link to an image to show the user",
				suggestion?.Image?.ToString(),
				false));

		return modal;
	}

	public static DiscordEmbedBuilder BuildExceptionEmbed(Exception exception)
	{
		DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
			.WithTitle("Whoops, something happened.")
			.WithDescription("sorry about that")
			.WithColor(DiscordColor.Red)
			.AddField(exception.GetType().Name, exception.Message)
			.WithFooter(
				"the developers got a notification about this error and *should* fix it soon so it doesnt happen again");

		if (exception is BadRequestException bre)
			embed.AddField("More info", $"```\n{bre.JsonMessage}\n```\n```json\n{bre.Errors}```");

		return embed;
	}
}