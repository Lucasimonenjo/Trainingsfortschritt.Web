namespace Trainingsfortschritt.Core.Models;

public class ExerciseOverviewItem
{
    public int ExerciseId { get; set; }
    public string Name { get; set; } = "";

    public double? Weight { get; set; }
    public int? Reps { get; set; }
    public string? RepsDisplay { get; set; }

    public DateTime? Date { get; set; }

    public string? Notes { get; set; }
    public string? Tags { get; set; }

    public double? TargetWeight { get; set; }
    public int? TargetReps { get; set; }

    public int GoalsReachedCount { get; set; }

    public double ProgressPercent { get; set; }

    public string ProgressText => $"{ProgressPercent:F0}%";

    public string LatestInfo =>
        Weight.HasValue ? $"{Weight} kg · {RepsDisplay}" : "Noch kein Set";

    public string DateInfo =>
        Date?.ToString("dd.MM.yyyy") ?? "";
    public int TargetSets { get; set; }

    public string TargetDisplay =>
    $"{TargetSets}×{TargetReps}";
}