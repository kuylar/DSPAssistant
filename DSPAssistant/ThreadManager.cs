using DSharpPlus.Entities;

namespace DSPAssistant;

public static class ThreadManager
{
	public static Dictionary<ulong, bool> FollowedThreadIds = new();

	public static bool TryFollowThread(DiscordThreadChannel eventArgsThread)
	{
		if (FollowedThreadIds.ContainsKey(eventArgsThread.Id))
			return false;
		FollowedThreadIds.Add(eventArgsThread.Id, false);
		return true;
	}

	public static bool ShouldRespondToMessage(ulong channelId)
	{
		if (FollowedThreadIds.TryGetValue(channelId, out bool res))
		{
			FollowedThreadIds[channelId] = true;
			return !res;
		}
		return false;
	}
}