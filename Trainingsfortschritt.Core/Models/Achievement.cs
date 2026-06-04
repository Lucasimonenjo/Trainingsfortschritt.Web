namespace Trainingsfortschritt.Core.Models;

public class Achievement
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Rarity { get; set; } = "COMMON";
    // COMMON, RARE, EPIC, IMPOSSIBLE, SPECIAL

    public bool IsUnlocked { get; set; }

    public DateTime? UnlockedDate { get; set; }
}