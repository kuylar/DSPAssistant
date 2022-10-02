using DSharpPlus;
using DSharpPlus.ButtonCommands;

namespace DSPAssistant.ButtonCommands;

public class ListPaginationModule : ButtonCommandModule
{
	[ButtonCommand("listSuggestions")]
	public async Task Vote(ButtonContext context, int page)
	{
		DatabaseContext db = Utils.CreateDatabaseContext();
		await context.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
			db.GetSuggestionsListEmbed(page));
	}
}