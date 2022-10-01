using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using FuzzySharp;

namespace DSPAssistant.SlashCommands;

public class SlashCommandModule : ApplicationCommandModule
{
	[SlashCommand("add-suggestion", "Add a new auto-suggestion", Permissions.ModerateMembers)]
	public async Task AddSuggestion(InteractionContext context,
		[Option("id", "The unique ID of this suggestion")]
		string id,
		[Option("title", "The title to show to the user")]
		string title,
		[Option("description", "A long description of what the user should do")]
		string description,
		[Option("code-example", "Tiny code example to guide the users. NO SPOONFEEDING!!!")]
		string? codeExample = null,
		[Option("image", "Image to show at the end")]
		DiscordAttachment? image = null)
	{
		DatabaseContext db = Utils.CreateDatabaseContext();
		if (db.Suggestions.Any(x => x.Id == id))
		{
			await context.CreateResponseAsync($"An item with the ID `{id}` already exists", true);
			return;
		}

		Suggestion suggestion = new()
		{
			Id = id,
			Synopsis = title,
			Description = description,
			CodeExample = codeExample,
			Image = image is not null ? new Uri(image.Url) : null,
			RelatedProblems = Array.Empty<string>()
		};
		db.Suggestions.Add(suggestion);
		await db.SaveChangesAsync();
		await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
			new DiscordInteractionResponseBuilder()
				.WithContent("Added the following suggestion")
				.AddEmbed(suggestion.BuildMessage().Embed));
	}

	[SlashCommand("add-keyword-match", "Add a keyword so that it matches a suggestion", Permissions.ModerateMembers)]
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
		await context.CreateResponseAsync($"Done! From now on, when I see '{keyword}', I will suggest '{suggestion.Synopsis}'");
	}
}

public class SuggestionAutocompleteProvider : IAutocompleteProvider
{
	public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
	{
		DatabaseContext db = Utils.CreateDatabaseContext();
		IEnumerable<Suggestion> suggestions = db.Suggestions.ToList()
			.OrderBy(x => Fuzz.Ratio(ctx.OptionValue.ToString(), x.Synopsis))
			.Take(10);
		return Task.FromResult(suggestions.Select(x =>
			new DiscordAutoCompleteChoice(x.Synopsis, x.Id)));
	}
}