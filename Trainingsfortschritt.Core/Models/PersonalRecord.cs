using SQLite;

namespace Trainingsfortschritt.Core.Models;

public class PersonalRecord
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int ExerciseId { get; set; }

    public double Weight { get; set; }

    public int Reps { get; set; }

    public DateTime Date { get; set; }
}