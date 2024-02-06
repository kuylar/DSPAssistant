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

	[SlashCommand("suggest", "Suggest a common problem")]
	public async Task Edit(InteractionContext context,
		[Option("suggestion", "The suggestion to suggest", autocomplete: true)]
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

		await context.CreateResponseAsync(suggestion.BuildMessage(context.Member ?? context.User).Embed);
	}
}