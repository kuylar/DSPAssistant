using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DSPAssistant.SlashCommands;

public class AboutCommandModule : ApplicationCommandModule
{
	[SlashCommand("about", "About me")]
	public async Task AboutCommand(InteractionContext context)
	{
		await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
			new DiscordInteractionResponseBuilder()
				.AddEmbed(new DiscordEmbedBuilder()
					.WithTitle("About DSPAssistant")
					.WithDescription(
						"I'm an advanced AI bot who is definitely running on machine learning and stuff to provide quick answers to common problems over at <#1019667931380072608>!\n" + 
						"||spoilers: its not really ML/AI its literally just string matching||")
					.AddField("Author", "<@310651646302486528>", true)
					.AddField("GitHub link", "https://github.com/kuylar/DSPAssistant", true)
					.AddField("How cool are you", $"I'd say that you are about {context.User.Id.ToString()[13..15]}% cool", true)));
	}
}