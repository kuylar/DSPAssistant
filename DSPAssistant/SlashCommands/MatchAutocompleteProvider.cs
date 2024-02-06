using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using FuzzySharp;

namespace DSPAssistant.SlashCommands;

public class MatchAutocompleteProvider : IAutocompleteProvider
{
	public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
	{
		DatabaseContext db = Utils.CreateDatabaseContext();
		IEnumerable<Match> suggestions = db.Matches.ToList()
			.OrderBy(x => Fuzz.Ratio(ctx.OptionValue.ToString(), x.Keyword))
			.Take(10);
		return Task.FromResult(suggestions.Select(x =>
			new DiscordAutoCompleteChoice(x.Keyword + " -> " + x.Item, x.Id.ToString())));
	}
}