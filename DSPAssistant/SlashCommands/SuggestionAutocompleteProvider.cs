using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using FuzzySharp;

namespace DSPAssistant.SlashCommands;

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