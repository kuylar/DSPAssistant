using System.ComponentModel.DataAnnotations;

namespace DSPAssistant;

public class SuggestionEdit
{
	[Key] public Guid Id { get; set; }
	public DateTimeOffset When { get; set; }
	public ulong EditedBy { get; set; }
	public Suggestion? Suggestion { get; set; }
}