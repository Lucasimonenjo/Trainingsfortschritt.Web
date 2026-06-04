using SQLite;

namespace Trainingsfortschritt.Core.Models;

public class GoalHistory
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int ExerciseId { get; set; }

    // NEU: Übungsname direkt speichern, damit die Historie unabhängig von der Übungstabelle ist
    public string ExerciseName { get; set; } = string.Empty;

    public double TargetWeight { get; set; }
    public string? TargetRepsDisplay { get; set; }
    public int? TargetReps { get; set; }

    // Datum des Sets, das das Ziel erreicht hat
    public DateTime DateReached { get; set; }

    public int DaysNeeded { get; set; }
    public int WorkoutsNeeded { get; set; }
    public int TargetSets { get; set; }
}
