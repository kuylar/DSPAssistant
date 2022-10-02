using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DSPAssistant;

public class DatabaseContext : DbContext
{
	public DbSet<Suggestion> Suggestions { get; set; }
	public DbSet<Match> Matches { get; set; }
	public DbSet<Vote> Votes { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder.UseNpgsql(Utils.GetConnectionString());
	}

	public int GetVotesForItem(string item) => Votes.Where(x => x.Item == item).Sum(x => x.Value);

	public void VoteForItem(Vote vote)
	{
		Vote? existingVote = Votes.Find(vote.VoteId);

		if (existingVote is not null)
		{
			Log.Information("{User} voted ({Value}) for {Item}. Updating their existing vote",
				vote.User,
				vote.Value,
				vote.Item);
			existingVote.Value = vote.Value;
		}
		else
		{
			Log.Information("{User} voted ({Value}) for {Item}",
				vote.User,
				vote.Value,
				vote.Item);
			Votes.Add(vote);
		}

		SaveChanges();
	}

	public IEnumerable<Suggestion> GetItemsFromMatches(string message)
	{
		IEnumerable<Match> match = Matches.ToList().Where(x => message.Contains(x.Keyword.ToLower()));
		return match.Select(x => Suggestions.Find(x.Item)).Where(x => x != null)!;
	}

	public DiscordInteractionResponseBuilder GetSuggestionsListEmbed(int page)
	{
		List<Suggestion> suggestions = Suggestions
			.Skip(page * 6)
			.Take(6)
			.ToList();
		int amount = Suggestions.Count();
		int maxPages = (int)Math.Ceiling(amount / 6f);
		DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
			.WithTitle("All auto-suggestions")
			.WithFooter($"There are a total of {amount} suggestions | Page {page + 1} / {maxPages}");
		foreach (Suggestion suggestion in suggestions)
			embed.AddField(
				suggestion.Synopsis,
				$"By <@{suggestion.CreatedBy}>\n**{GetVotesForItem(suggestion.Id)}** votes",
				true);

		DiscordSelectComponentOption[] previews = suggestions
			.Select(x => new DiscordSelectComponentOption(x.Synopsis, x.Id))
			.ToArray();

		DiscordInteractionResponseBuilder dirb = new();
		dirb.AddEmbed(embed);
		dirb.AddComponents(
			new DiscordButtonComponent(
				ButtonStyle.Primary,
				Utils.GetButtonId("listSuggestions", page - 1),
				"Previous Page",
				page == 0),
			new DiscordButtonComponent(
				ButtonStyle.Primary,
				Utils.GetButtonId("listSuggestions", page + 1),
				"Next Page",
				page + 2 > maxPages));
		dirb.AddComponents(
			new DiscordSelectComponent(
				Utils.GetButtonId("showFix"),
				"Select one to preview",
				previews));

		return dirb;
	}
}