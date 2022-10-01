using System.ComponentModel.DataAnnotations;

namespace DSPAssistant;

public class Match
{
	[Key]
	public Guid Id { get; set; }

	public string Keyword { get; set; }
	public string Item { get; set; }
}