namespace Trainingsfortschritt.Web.Models;

public class GoalHistoryDisplayItem
{
    public string ExerciseName { get; set; } = "";

    public double TargetWeight { get; set; }

    public int TargetReps { get; set; }

    public string? TargetRepsDisplay { get; set; }

    public DateTime DateReached { get; set; }

    public int DaysNeeded { get; set; }

    public int WorkoutsNeeded { get; set; }
    public int Sets { get; set; }
    public int Reps { get; set; }
}