using Trainingsfortschritt.Core.Models;

public class ImportModel
{
    public List<Exercise> Exercises { get; set; } = new();
    public List<ExerciseSet> Sets { get; set; } = new();
    public List<GoalHistory> GoalHistory { get; set; } = new();
}