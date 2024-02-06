using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Exceptions;
using Serilog;

namespace DSPAssistant.SlashCommands;

[SlashCommandGroup("autosuggestions", "Automated suggestions for the support channel")]
[SlashCommandPermissions(Permissions.ModerateMembers)]
public class SlashCommandModule : ApplicationCommandModule
{
	[SlashCommand("create", "Add a new auto-suggestion")]
	public async Task Create(InteractionContext context)
	{
		await context.CreateResponseAsync(InteractionResponseType.Modal,
			Utils.BuildCreateSuggestionModal());
	}

	[SlashCommand("list", "Show all the auto-suggestions")]
	public async Task List(InteractionContext context)
	{
		DatabaseContext db = Utils.CreateDatabaseContext();
		await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
			db.GetSuggestionsListEmbed(0));
	}

	[SlashCommand("delete", "Delete an auto-suggestion")]
	public async Task Delete(InteractionContext context,
		[Option("suggestion", "The suggestion to delete", autocomplete: true)]
		[Autocomplete(typeof(SuggestionAutocompleteProvider))]
		string suggestionId)
	{
		DatabaseContext db = Utils.CreateDatabaseContext();
		Suggestion? suggestion = await db.Suggestions.FindAsync(suggestionId);

		if (suggestion is null)
		{
			await context.CreateResponseAsync($"Failed to find a suggestion with ID `{suggestionId}`", true);
			return;
		}

		db.DeleteSuggestion(suggestion, context.User.Id);
		await context.CreateResponseAsync($"Removed auto-suggestion '`[{suggestion.Id}]` {suggestion.Synopsis}'");
	}

	[SlashCommand("edit", "Edit an auto-suggestion")]
	public async Task Edit(InteractionContext context,
		[Option("suggestion", "The suggestion to edit", autocomplete: true)]
		[Autocomplete(typeof(SuggestionAutocompleteProvider))]
		string suggestionId)
	{
		DatabaseContext db = Utils.CreateDatabaseContext();
		Suggestion? suggestion = await db.Suggestions.FindAsync(suggestionId);

		if (suggestion is null)
		{
			await context.CreateResponseAsync($"Failed to find a suggestion with ID `{suggestionId}`", true);
			return;
		}

		await context.CreateResponseAsync(InteractionResponseType.Modal, Utils.BuildCreateSuggestionModal(suggestion));
	}

	[SlashCommand("add-match", "Add a keyword that matches a suggestion")]
	public async Task AddMatch(InteractionContext context,
		[Option("keyword", "Keyword to match the suggestion")]
		string keyword,
		[Option("suggestion", "The suggestion to match", autocomplete: true)]
		[Autocomplete(typeof(SuggestionAutocompleteProvider))]
		string suggestionId)
	{
		DatabaseContext db = Utils.CreateDatabaseContext();
		Suggestion? suggestion = db.Suggestions.Find(suggestionId);
		if (suggestion is null)
		{
			await context.CreateResponseAsync($"Suggestion `{suggestionId}` was not found.", true);
			return;
		}

		db.Matches.Add(new Match
		{
			Id = Guid.NewGuid(),
			Keyword = keyword,
			Item = suggestion.Id
		});
		await db.SaveChangesAsync();
		await context.CreateResponseAsync(
			$"Done! From now on, when I see '{keyword}', I will suggest '{suggestion.Synopsis}'");
	}

	[SlashCommand("delete-match", "Delete a keyword that matches a suggestion")]
	public async Task DeleteMatch(InteractionContext context,
		[Option("match", "Keyword - suggestion combo. Use autocomplete", autocomplete: true)]
		[Autocomplete(typeof(MatchAutocompleteProvider))]
		string matchId)
	{
		DatabaseContext db = Utils.CreateDatabaseContext();
		Match? match = db.Matches.Find(Guid.Parse(matchId));
		if (match is null)
		{
			await context.CreateResponseAsync($"Match `{matchId}` was not found.", true);
			return;
		}

		db.Matches.Remove(match);
		await db.SaveChangesAsync();
		await context.CreateResponseAsync("https://tenor.com/view/disintegrating-funny-thanos-gif-22399978");
	}
}
