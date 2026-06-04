namespace Trainingsfortschritt.Core.Models;

public class AchievementItem
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Rarity { get; set; } = "COMMON";

    public bool IsUnlocked { get; set; }

    public DateTime? UnlockedDate { get; set; }

    // =========================
    // PROGRESS SYSTEM
    // =========================

    public double Progress { get; set; }

    public double CurrentValue { get; set; }

    public double TargetValue { get; set; }

    public string ProgressText =>
        $"{CurrentValue:0} / {TargetValue:0}";

    // =========================
    // HEX COLORS (plattformneutral)
    // =========================

    public string Color =>
        Rarity switch
        {
            "COMMON" => "#9E9E9E",
            "RARE" => "#4CAF50",
            "EPIC" => "#9C27B0",
            "LEGENDARY" => "#FFD700",
            "IMPOSSIBLE" => "#F44336",
            "SPECIAL" => "#FF9800",
            _ => "#9E9E9E"
        };

    public string RarityColor =>
        Rarity switch
        {
            "COMMON" => "#808080",
            "RARE" => "#ADD8E6",
            "EPIC" => "#800080",
            "LEGENDARY" => "#FFD700",
            "IMPOSSIBLE" => "#FF0000",
            "SPECIAL" => "#008000",
            _ => "#808080"
        };
}