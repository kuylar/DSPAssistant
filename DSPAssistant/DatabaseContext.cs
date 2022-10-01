using FuzzySharp;
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
}