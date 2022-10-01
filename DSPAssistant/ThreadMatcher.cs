using DSharpPlus.Entities;

namespace DSPAssistant;

public static class ThreadMatcher
{
	public static List<Suggestion> MatchMessage(DiscordMessage message)
	{
		string query = message.Channel.Name + " " + message.Content;
		DatabaseContext db = Utils.CreateDatabaseContext();
		return db.GetItemsFromMatches(query.ToLower()).DistinctBy(x => x.Id).ToList();
	}
}