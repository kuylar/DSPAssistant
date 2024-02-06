using System.ComponentModel.DataAnnotations;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;

namespace DSPAssistant;

public class Suggestion
{
	[Key] public string Id { get; set; }
	public string Synopsis { get; set; }
	public string Description { get; set; }
	public string? CodeExample { get; set; }
	public Uri? Image { get; set; }
	public string[] RelatedProblems { get; set; }
	public ulong CreatedBy { get; set; }

	public DiscordMessageBuilder BuildMessage(DiscordUser? suggester = null)
	{
		StringBuilder description = new();

		description.AppendLine(Description);

		if (CodeExample is not null)
			description.AppendLine("\n**Code Example:**")
				.AppendLine("```cs")
				.AppendLine(CodeExample)
				.AppendLine("```");

		DiscordEmbedBuilder embed = new();
		embed
			.WithTitle($"Automated suggestion: {Synopsis}")
			.WithDescription(description.ToString())
			.WithColor(DiscordColor.Blurple);

		if (suggester == null)
		{
			embed.WithTitle($"Automated suggestion: {Synopsis}")
				.WithFooter(
					"This action was performed automatically. Please use the buttons below to vote on your experience");
		}
		else
		{
			embed.WithTitle(Synopsis)
				.WithFooter($"Suggested by {(suggester is DiscordMember mem ? mem.DisplayName : suggester.Username)}");
		}

		if (Image is not null)
			embed.WithImageUrl(Image);


		DiscordMessageBuilder? message = new DiscordMessageBuilder()
			.AddEmbed(embed);
		if (suggester == null)
			message.AddComponents(
				new DiscordButtonComponent(ButtonStyle.Success, Utils.GetButtonId("vote", 1, Id),
					"Problem fixed!"),
				new DiscordButtonComponent(ButtonStyle.Danger, Utils.GetButtonId("vote", -1, Id),
					"Did not help"));
		return message;
	}
}