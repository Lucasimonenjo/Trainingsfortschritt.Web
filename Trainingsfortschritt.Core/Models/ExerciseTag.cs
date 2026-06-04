namespace Trainingsfortschritt.Core.Models;

public class ExerciseTag
{
    public string Name { get; set; } = string.Empty;

    // nur UI Zustand
    public bool IsSelected { get; set; }
}