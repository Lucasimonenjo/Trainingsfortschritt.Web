using System.Collections.ObjectModel;
using Trainingsfortschritt.Core.Models;

namespace Trainingsfortschritt.Core.Models;

public class ExerciseGroup : ObservableCollection<Exercise>
{
    public string Name { get; }
    public bool IsExpanded { get; set; }

    public ExerciseGroup(string name, IEnumerable<Exercise> exercises, bool expanded = false)
        : base(exercises)
    {
        Name = name;
        IsExpanded = expanded;
    }
}
