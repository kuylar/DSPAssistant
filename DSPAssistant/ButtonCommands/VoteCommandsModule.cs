using DSharpPlus;
using DSharpPlus.ButtonCommands;
using DSharpPlus.Entities;

namespace DSPAssistant.ButtonCommands;

public class VoteCommandsModule : ButtonCommandModule
{
	[ButtonCommand("vote")]
	public async Task Vote(ButtonContext context, int value, string item)
	{
		DatabaseContext db = Utils.CreateDatabaseContext();
		db.VoteForItem(new Vote(context.User, value, item));
		int newVotes = db.GetVotesForItem(item);
		await context.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
			new DiscordInteractionResponseBuilder()
				.AsEphemeral()
				.WithContent($"Thanks for your vote! This suggestion now has {newVotes} points."));
	}
}