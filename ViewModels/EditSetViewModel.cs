using System.Collections.ObjectModel;
using Trainingsfortschritt.Core.Models;
using Trainingsfortschritt.Core.Services;
using Trainingsfortschritt.Web.Services;

namespace Trainingsfortschritt.Web.ViewModels;

public class EditSetViewModel
{
    private readonly IndexedDbService _db;

    public EditSetViewModel(IndexedDbService db)
    {
        _db = db;
    }

    public int SetId { get; set; }

    // 🔥 gehört zur Übung
    public int ExerciseId { get; set; }

    public double Weight { get; set; }

    public string RepsInput { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public string? SeatSetting { get; set; }

    public bool IsLoaded { get; set; }

    private string? _lastGoalMessage;

    // =========================
    // TAGS
    // =========================
    public class TagItem
    {
        public string Name { get; set; } = string.Empty;

        public bool IsSelected { get; set; }
    }

    public List<TagItem> Tags { get; set; } = new()
    {
        new() { Name = "PR" },
        new() { Name = "Heavy" },
        new() { Name = "Tired" },
        new() { Name = "Pain" },
        new() { Name = "Strong" },
        new() { Name = "Warmup" },
        new() { Name = "Failure" },
        new() { Name = "Deload" },
        new() { Name = "Technique" },
        new() { Name = "Pump" },
        new() { Name = "Superset" },
        new() { Name = "Drop Set" },
        new() { Name = "Slow Tempo" },
        new() { Name = "Fast Tempo" }
    };

    // =========================
    // LOAD
    // =========================
    public async Task LoadAsync()
    {
        var set = await _db.GetSetByIdAsync(SetId);

        if (set == null)
            return;

        // 🔥 ExerciseId merken
        ExerciseId = set.ExerciseId;

        Weight = set.Weight;
        RepsInput = set.RepsDisplay ?? string.Empty;
        Notes = set.Notes;
        SeatSetting = set.SeatSetting;

        foreach (var t in Tags)
            t.IsSelected = false;

        if (!string.IsNullOrWhiteSpace(set.Tags))
        {
            var selected = set.Tags
                .Split(',')
                .Select(x => x.Trim());

            foreach (var tag in Tags)
                tag.IsSelected = selected.Contains(tag.Name);
        }

        IsLoaded = true;
    }

    // =========================
    // SAVE
    // =========================
    public async Task SaveAsync()
    {
        var set = await _db.GetSetByIdAsync(SetId);

        if (set == null)
            return;

        // =========================
        // REP PARSING
        // =========================

        int sets = 1;
        int repsPerSet = 0;
        int totalReps = 0;

        if (!string.IsNullOrWhiteSpace(RepsInput) &&
            RepsInput.Contains("x"))
        {
            var parts = RepsInput.Split('x');

            sets = int.Parse(parts[0].Trim());
            repsPerSet = int.Parse(parts[1].Trim());

            totalReps = sets * repsPerSet;
        }
        else
        {
            repsPerSet =
                int.TryParse(RepsInput, out var r)
                    ? r
                    : 0;

            totalReps = repsPerSet;
        }

        // =========================
        // UPDATE SET
        // =========================

        set.Weight = Weight;
        set.Reps = totalReps;
        set.Sets = sets;
        set.RepsPerSet = repsPerSet;

        set.RepsDisplay = RepsInput;

        set.Notes = Notes;
        set.SeatSetting = SeatSetting;

        set.Tags = string.Join(",",
            Tags
                .Where(t => t.IsSelected)
                .Select(t => t.Name));

        // =========================
        // GOAL CHECK (NUR EINMAL)
        // =========================

        var exercise =
            await _db.GetExerciseByIdAsync(set.ExerciseId);

        if (exercise != null)
        {
            bool hasWeight =
                exercise.TargetWeight > 0;

            bool hasReps =
                exercise.TargetReps > 0;

            bool reached =
                (hasWeight
                    ? set.Weight >= exercise.TargetWeight
                    : true)
                &&
                (hasReps
                    ? set.Reps >= exercise.TargetReps
                    : true);

            if (reached)
            {
                _lastGoalMessage =
                    hasReps
                        ? $"{exercise.TargetWeight} kg · {exercise.TargetSets}x{exercise.TargetReps}"
                        : $"{exercise.TargetWeight} kg";

                // TARGET RESET
                exercise.TargetWeight = 0;
                exercise.TargetSets = 0;
                exercise.TargetReps = 0;

                await _db.UpdateExerciseAsync(exercise);
            }
        }

        // =========================
        // SAVE SET
        // =========================

        await _db.UpdateSetAsync(set);

        // 🔥 Reload
        await LoadAsync();
    }

    // =========================
    // TAG TOGGLE
    // =========================
    public void ToggleTag(TagItem tag)
    {
        if (tag == null)
            return;

        tag.IsSelected = !tag.IsSelected;
    }

    // =========================
    // GOAL MESSAGE
    // =========================
    public Task<string?> GetGoalMessageAsync()
    {
        return Task.FromResult(_lastGoalMessage);
    }
}