using Trainingsfortschritt.Core.Models;
public class ExerciseGroup : List<ExerciseOverviewItem>
{
    public string Name { get; set; }
    public bool IsExpanded { get; set; }

    public ExerciseGroup(string name, IEnumerable<ExerciseOverviewItem> items)
        : base(items)
    {
        Name = name;
    }
}