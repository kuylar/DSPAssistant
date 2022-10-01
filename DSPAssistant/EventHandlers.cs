using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
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
}