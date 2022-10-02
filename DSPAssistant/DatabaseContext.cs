using System.Text.Json;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DSPAssistant;

public class DatabaseContext : DbContext
{
	public DbSet<Suggestion> Suggestions { get; set; }
	public DbSet<SuggestionEdit> EditHistory { get; set; }
	public DbSet<Match> Matches { get; set; }
	public DbSet<Vote> Votes { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder.UseNpgsql(Utils.GetConnectionString());
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<SuggestionEdit>()
			.Property(x => x.Suggestion)
			.HasConversion(
				x => JsonSerializer.Serialize(x, new JsonSerializerOptions()),
				x => JsonSerializer.Deserialize<Suggestion>(x, new JsonSerializerOptions())!);
	}

	public void CreateSuggestion(Suggestion s, ulong createdBy)
	{
		Suggestion? suggestion = Suggestions.Find(s.Id);

		if (suggestion is not null)
			throw new KeyNotFoundException("Another suggestion with the same ID already exists");

		EditHistory.Add(new SuggestionEdit
		{
			EditedBy = createdBy,
			Suggestion = s,
			When = DateTimeOffset.UtcNow
		});
		Suggestions.Add(s);
		SaveChanges();
	}

	public void UpdateSuggestion(Suggestion s, ulong editedBy)
	{
		Suggestion? suggestion = Suggestions.AsNoTracking().FirstOrDefault(x => s.Id == x.Id);

		if (suggestion is null)
			throw new KeyNotFoundException("Cannot edit a suggestion that doesn't exist");

		EditHistory.Add(new SuggestionEdit
		{
			EditedBy = editedBy,
			Suggestion = s,
			When = DateTimeOffset.UtcNow
		});
		Suggestions.Update(s);
		SaveChanges();
	}

	public void DeleteSuggestion(Suggestion s, ulong deletedBy)
	{
		Suggestion? suggestion = Suggestions.Find(s.Id);

		if (suggestion is null)
			throw new KeyNotFoundException("Cannot delete a suggestion that doesn't exist");

		EditHistory.Add(new SuggestionEdit
		{
			EditedBy = deletedBy,
			Suggestion = null,
			When = DateTimeOffset.UtcNow
		});
		Suggestions.Remove(s);
		SaveChanges();
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