using System.Text;
using DSharpPlus;
using DSharpPlus.ButtonCommands;
using DSharpPlus.ButtonCommands.EventArgs;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Serilog;

namespace DSPAssistant;

public static class EventHandlers
{
	public static async Task OnThreadCreated(DiscordClient sender, ThreadCreateEventArgs eventArgs)
	{
		if (!ThreadManager.TryFollowThread(eventArgs.Thread))
			// no we dont want duplicate event calls
			return;

		Log.Information("Thread created: {ThreadName}", eventArgs.Thread.Name);
	}

	public static async Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs eventArgs)
	{
		if (!ThreadManager.ShouldRespondToMessage(eventArgs.Channel.Id))
			return;

		List<Suggestion> fixes = ThreadMatcher.MatchMessage(eventArgs.Message);

		switch (fixes.Count)
		{
			case 1:
			{
				Suggestion fix = fixes[0];
				DiscordMessageBuilder message = fix.BuildMessage();

				// other possible fixes
				DiscordSelectComponentOption[] otherFixes = fixes
					.Skip(1)
					.Take(25)
					.Select(x => new DiscordSelectComponentOption(x.Synopsis, x.Id))
					.ToArray();
				if (otherFixes.Length > 0)
					message.AddComponents(
						new DiscordSelectComponent("showFix", "Click here for other possible solutions!", otherFixes));

				await eventArgs.Channel.SendMessageAsync(message);
				break;
			}
			case > 1:
			{
				DiscordMessageBuilder message = new DiscordMessageBuilder()
					.WithContent(
						"Hello, I'm an automated bot to help people with common issues. Here's a list of stuff that I think are related to your problem.");

				DiscordSelectComponentOption[] otherFixes = fixes
					.Take(25)
					.Select(x => new DiscordSelectComponentOption(x.Synopsis, x.Id))
					.ToArray();

				message.AddComponents(
					new DiscordSelectComponent(Utils.GetButtonId("showFix"), "Choose one!", otherFixes));

				await eventArgs.Channel.SendMessageAsync(message);
				break;
			}
		}
	}

	public static async Task OnReady(DiscordClient sender, ReadyEventArgs eventArgs)
	{
		Log.Information("Connected as {Username}#{Discriminator}",
			sender.CurrentUser.Username,
			sender.CurrentUser.Discriminator);
	}

	public static async Task OnModalSubmitted(DiscordClient sender, ModalSubmitEventArgs eventArgs)
	{
		DatabaseContext db = Utils.CreateDatabaseContext();
		Suggestion s = new()
		{
			CreatedBy = eventArgs.Interaction.User.Id,
			RelatedProblems = Array.Empty<string>()
		};

		foreach ((string? key, string? value) in eventArgs.Values)
		{
			switch (key)
			{
				case "id":
					s.Id = value;
					break;
				case "title":
					s.Synopsis = value;
					break;
				case "description":
					s.Description = value;
					break;
				case "code":
					if (!string.IsNullOrWhiteSpace(value))
						s.CodeExample = value;
					break;
				case "image":
					try
					{
						if (!string.IsNullOrWhiteSpace(value))
							s.Image = new Uri(value);
					}
					catch
					{
					}

					break;
			}
		}

		if (db.Suggestions.Any(x => x.Id == s.Id))
		{
			await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
				new DiscordInteractionResponseBuilder()
					.WithContent("Failed to create a suggestion.\n> Another item with the same ID already exists"));
			return;
		}

		string[] args = eventArgs.Interaction.Data.CustomId.Split(" ");
		try
		{
			if (args[0] == "editSuggestion")
			{
				s.Id = args[1];
				db.UpdateSuggestion(s, eventArgs.Interaction.User.Id);
				await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
					new DiscordInteractionResponseBuilder()
						.WithContent("Successfully edited the following suggestion:")
						.AddEmbed(s.BuildMessage().Embed));
			}
			else
			{
				db.CreateSuggestion(s, eventArgs.Interaction.User.Id);
				await db.SaveChangesAsync();
				await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
					new DiscordInteractionResponseBuilder()
						.WithContent("Successfully created the following suggestion:")
						.AddEmbed(s.BuildMessage().Embed));
			}
		}
		catch (Exception e)
		{
			Log.Error(
				e,
				"Modal {ModalName} errored!",
				eventArgs.Interaction.Data.CustomId);
			await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
				new DiscordInteractionResponseBuilder()
					.WithContent("Failed to create a suggestion")
					.AddEmbed(Utils.BuildExceptionEmbed(e)));
		}
	}

	public static async Task OnButtonCommandErrored(ButtonCommandsExtension sender,
		ButtonCommandErrorEventArgs eventArgs)
	{
		Log.Error(
			eventArgs.Exception,
			"Button command {CommandName} errored!",
			eventArgs.CommandName);

		DiscordEmbedBuilder embed = Utils.BuildExceptionEmbed(eventArgs.Exception);

		await eventArgs.Context.Interaction.CreateResponseAsync(
			InteractionResponseType.ChannelMessageWithSource,
			new DiscordInteractionResponseBuilder()
				.AddEmbed(embed));
	}

	public static async Task OnSlashCommandErrored(SlashCommandsExtension sender,
		SlashCommandErrorEventArgs eventArgs)
	{
		Log.Error(
			eventArgs.Exception,
			"Slash command {CommandName} errored!",
			eventArgs.Context.Interaction.Data.Name);

		DiscordEmbedBuilder embed = Utils.BuildExceptionEmbed(eventArgs.Exception);

		await eventArgs.Context.Interaction.CreateResponseAsync(
			InteractionResponseType.ChannelMessageWithSource,
			new DiscordInteractionResponseBuilder()
				.AddEmbed(embed));
	}
}