using SQLite;

namespace Trainingsfortschritt.Core.Models;

public class Exercise
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int? ParentExerciseId { get; set; }

    [NotNull]
    public string Name { get; set; } = string.Empty;

    // Zielgewicht (optional)
    public double TargetWeight { get; set; }

    // Ziel-Wiederholungen als Anzeige (z. B. "3x8")

    // Ziel-Wiederholungen als Gesamtzahl (z. B. 24)
    public int TargetReps { get; set; }

    // Zeitpunkt, an dem das Ziel gesetzt wurde
    public DateTime? TargetSetDate { get; set; }

    // 🔥 NEU: wie oft das Ziel für diese Übung erreicht wurde
    public int GoalsReachedCount { get; set; }
    public int TargetSets { get; set; }

}

