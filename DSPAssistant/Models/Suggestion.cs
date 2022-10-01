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

	public DiscordMessageBuilder BuildMessage()
	{
		StringBuilder description = new();

		description.AppendLine(Description);

		if (CodeExample is not null)
			description.AppendLine("**Code Example:**\n")
				.AppendLine("```cs")
				.AppendLine(CodeExample)
				.AppendLine("```");

		DiscordEmbedBuilder embed = new();
		embed
			.WithTitle($"Automated suggestion: {Synopsis}")
			.WithDescription(description.ToString())
			.WithFooter(
				"This action was performed automatically. Please use the buttons below to vote on your experience");

		if (Image is not null)
			embed.WithImageUrl(Image);

		DiscordMessageBuilder? message = new DiscordMessageBuilder()
			.AddEmbed(embed)
			.AddComponents(
				new DiscordButtonComponent(ButtonStyle.Success, Utils.GetButtonId("vote", 1, Id),
					"Problem fixed!"),
				new DiscordButtonComponent(ButtonStyle.Danger, Utils.GetButtonId("vote", -1, Id),
					"Did not help"));
		return message;
	}
}