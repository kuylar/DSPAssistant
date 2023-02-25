using DSharpPlus;
using DSharpPlus.ButtonCommands;
using DSharpPlus.Entities;

namespace DSPAssistant.ButtonCommands;

public class SelfRoleCommandModule : ButtonCommandModule
{
	[ButtonCommand("selfrole:grant")]
	public async Task GrantNoUpdate(ButtonContext context, ulong id) => Grant(context, id, false);

	[ButtonCommand("selfrole:grantUpdate")]
	public async Task GrantUpdate(ButtonContext context, ulong id) => Grant(context, id, true);

	private async Task Grant(ButtonContext context, ulong id, bool update)
	{
		await context.Interaction.CreateResponseAsync(
			update
				? InteractionResponseType.DeferredMessageUpdate
				: InteractionResponseType.DeferredChannelMessageWithSource,
			new DiscordInteractionResponseBuilder().AsEphemeral());

		DiscordEmbedBuilder embed = new();

		try
		{
			if (context.Member.Roles.Any(x => x.Id == id))
			{
				await context.Member.RevokeRoleAsync(context.Guild.Roles[id]);
				embed.WithTitle(":wave: Success")
					.WithDescription($"Say goodbye to the <&{id}> role!")
					.WithColor(DiscordColor.Green);
			}
			else
			{
				await context.Member.GrantRoleAsync(context.Guild.Roles[id]);
				embed.WithTitle(":+1: Success")
					.WithDescription("Enjoy your new role!")
					.WithColor(DiscordColor.Green);
			}
		}
		catch (Exception e)
		{
			embed.WithTitle("<:skoll:1070613755127410708> Something went wrong...")
				.WithDescription($"{e.GetType().Name}: {e.Message}")
				.WithColor(DiscordColor.Red);
		}

		await context.Interaction.EditOriginalResponseAsync(
			new DiscordWebhookBuilder().AddEmbed(embed));
	}

	[ButtonCommand("selfrole:revoke")]
	public async Task Revoke(ButtonContext context, ulong id)
	{
		DiscordEmbedBuilder embed = new();

		try
		{
			await context.Member.RevokeRoleAsync(context.Guild.Roles[id]);
			embed.WithTitle("Success")
				.WithDescription($"Say goodbye to the <&{id}> role! :wave:")
				.WithColor(DiscordColor.Green);
		}
		catch (Exception e)
		{
			embed.WithTitle("<:skoll:1070613755127410708> Something went wrong...")
				.WithDescription($"{e.GetType().Name}: {e.Message}")
				.WithColor(DiscordColor.Red);
		}

		await context.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
			new DiscordInteractionResponseBuilder().AddEmbed(embed).AsEphemeral());
	}

	[ButtonCommand("selfrole:libdisc")]
	public async Task LibraryDevelopmentWarning(ButtonContext context, ulong id)
	{
		DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
			.WithTitle(":warning: Warning")
			.WithColor(DiscordColor.Yellow)
			.WithDescription(
				"<#379386901725052928> is **NOT** a channel to get help with the library. It is a channel for the library developers to discuss the changes to the DSharpPlus library.\n\nIf you're going to ask a question, ask in <#1019667931380072608>.\n\nAny support related questions will not be answered in <#379386901725052928>.\nAbusing this feature may lead to a ban.");

		await context.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
			new DiscordInteractionResponseBuilder()
				.AsEphemeral()
				.AddEmbed(embed)
				.AddComponents(
					new DiscordButtonComponent(ButtonStyle.Secondary, "_",
						"Please read the disclaimer (10s)", true))
				.AddComponents(
					new DiscordButtonComponent(ButtonStyle.Danger, "__",
						"I have read and understood what #lib-development is for", true)));

		await Task.Delay(TimeSpan.FromSeconds(10));

		await context.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
			.AddEmbed(embed)
			.AddComponents(
				new DiscordButtonComponent(ButtonStyle.Secondary, "_",
					"Please read the disclaimer", true))
			.AddComponents(
				new DiscordButtonComponent(ButtonStyle.Danger, Utils.GetButtonId("selfrole:grantUpdate", id),
					"I have read and understood what #lib-development is for")));
	}
}