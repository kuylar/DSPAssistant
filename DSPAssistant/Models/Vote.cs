using System.ComponentModel.DataAnnotations;
using DSharpPlus.Entities;

namespace DSPAssistant;

public class Vote
{
	[Key]
	public string VoteId { get; set; }
	public ulong User { get; set; }
	public int Value { get; set; }
	public string Item { get; set; }

	public Vote()
	{ }


	public Vote(DiscordUser user, int value, string item)
	{
		VoteId = $"{user.Id}:{item}";
		User = user.Id;
		Item = item;
		Value = value;
	}
}