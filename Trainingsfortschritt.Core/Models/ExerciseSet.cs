using SQLite;
using System.Linq;

namespace Trainingsfortschritt.Core.Models;

public class ExerciseSet
{
    [PrimaryKey, AutoIncrement]

    [Indexed]
    public int Id { get; set; }
    public int ExerciseId { get; set; }

    public double Weight { get; set; }
    public int Reps { get; set; }
    public string? RepsDisplay { get; set; }
    public string? SeatSetting { get; set; }
    public DateTime Date { get; set; }
    public string? Notes { get; set; }

    // DB: Kommagetrennt
    public string? Tags { get; set; }
    public bool IsPR { get; set; }

    // UI Helper
    [Ignore]
    public List<string> TagList =>
        string.IsNullOrWhiteSpace(Tags)
            ? new List<string>()
            : Tags.Split(',').ToList();
    public int Sets { get; set; }
    public int RepsPerSet { get; set; }
}