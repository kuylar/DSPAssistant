using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DSPAssistant.SlashCommands;

public class SelfRoleCommandModule : ApplicationCommandModule
{
	[SlashCommand("selfrole", "Give yourself a role", Permissions.ManageMessages)]
	public async Task SelfRoleCommand(InteractionContext context)
	{
		DiscordEmbedBuilder deployEmbed = new DiscordEmbedBuilder()
			.WithTitle("Deploying self role message")
			.WithColor(DiscordColor.Green)
			.WithImageUrl("https://i.kym-cdn.com/photos/images/newsfeed/001/274/974/93e.jpg");

		DiscordInteractionResponseBuilder dirb = new DiscordInteractionResponseBuilder()
			.AsEphemeral()
			.AddEmbed(deployEmbed);

		await context.CreateResponseAsync(dirb);

		DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
			.WithTitle("Self roles")
			.WithColor(DiscordColor.Blurple)
			.WithDescription("You can take any of the following roles!");

		DiscordMessageBuilder message = new DiscordMessageBuilder()
			.WithEmbed(embed)
			.AddComponents(
				new DiscordButtonComponent(ButtonStyle.Primary,
					Utils.GetButtonId("selfrole:grant", 379387961915080724),
					"Announcements", false, new DiscordComponentEmoji(379403717851742258)),
				new DiscordButtonComponent(ButtonStyle.Primary,
					Utils.GetButtonId("selfrole:libdisc", 1057954598683414619),
					"Library Development", false, new DiscordComponentEmoji(379389994097180675)));

		await context.Channel.SendMessageAsync(message);
	}
}