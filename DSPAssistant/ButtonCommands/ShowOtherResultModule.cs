using DSharpPlus;
using DSharpPlus.ButtonCommands;
using DSharpPlus.Entities;

namespace DSPAssistant.ButtonCommands;

public class ShowOtherResultModule : ButtonCommandModule
{
	[ButtonCommand("showFix")]
	public async Task ShowFix(ButtonContext context)
	{
		DatabaseContext db = Utils.CreateDatabaseContext();
		Suggestion? fix = await db.Suggestions.FindAsync(context.Values.First());
		if (fix is null)
			await context.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
				new DiscordInteractionResponseBuilder()
					.WithContent($"Couldn't find a suggestion with ID `{context.Values.First()}`"));
		else
			await context.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
				new DiscordInteractionResponseBuilder(fix.BuildMessage()));
	}
}