using Trainingsfortschritt.Core.Abstractions;
using Trainingsfortschritt.Core.Models;

namespace Trainingsfortschritt.Web.ViewModels;

public class ExerciseDetailViewModel
{
    private readonly IDatabaseService _databaseService;
    private readonly Func<string, Task<bool>> _confirm;

    public ExerciseDetailViewModel(
        IDatabaseService databaseService,
        Func<string, Task<bool>> confirm)
    {
        _databaseService = databaseService;
        _confirm = confirm;
    }

    // =========================
    // STATE
    // =========================

    public int ExerciseId { get; private set; }

    public string ExerciseName { get; private set; } = "Übung";

    public double TargetWeight { get; private set; }

    public int TargetSets { get; private set; }

    public int TargetReps { get; private set; }

    public List<ExerciseSet> Sets { get; private set; } = new();

    public bool IsLoaded { get; private set; }

    // =========================
    // LOAD
    // =========================

    public async Task LoadAsync(int exerciseId)
    {
        ExerciseId = exerciseId;

        var exercise =
            await _databaseService.GetExerciseByIdAsync(exerciseId);

        if (exercise == null)
            return;

        ExerciseName = exercise.Name ?? "Übung";

        TargetWeight = exercise.TargetWeight;
        TargetSets = exercise.TargetSets;
        TargetReps = exercise.TargetReps;

        Sets = (await _databaseService.GetSetsForExerciseAsync(exerciseId))
            .OrderByDescending(x => x.Date)
            .ToList();

        IsLoaded = true;
    }

    // =========================
    // ADD SET
    // =========================

    public async Task<string?> AddSetAsync(ExerciseSet set)
    {
        await _databaseService.AddSetAsync(set);

        Sets.Insert(0, set);

        return await CheckGoalAsync(set);
    }

    // =========================
    // UPDATE SET
    // =========================

    public async Task<string?> UpdateSetAsync(ExerciseSet set)
    {
        await _databaseService.UpdateSetAsync(set);

        await LoadAsync(ExerciseId);

        return await CheckGoalAsync(set);
    }

    // =========================
    // GOAL CHECK (FIXED + SAFE)
    // =========================

    private async Task<string?> CheckGoalAsync(ExerciseSet set)
    {
        var exercise =
            await _databaseService.GetExerciseByIdAsync(set.ExerciseId);

        if (exercise == null)
            return null;

        double targetWeight = exercise.TargetWeight;
        int targetSets = exercise.TargetSets;
        int targetReps = exercise.TargetReps;

        bool hasWeight = targetWeight > 0;
        bool hasReps = targetReps > 0;

        if (!hasWeight && !hasReps)
            return null;

        bool reached =
            (hasWeight ? set.Weight >= targetWeight : true) &&
            (hasReps ? set.Reps >= targetReps : true);

        if (!reached)
            return null;

        string targetDisplay =
            hasReps
                ? $"{targetWeight} kg · {targetSets}x{targetReps}"
                : $"{targetWeight} kg";

        // 🔥 TARGET RESET IM DB
        await _databaseService.SetExerciseTargetAsync(
            ExerciseId,
            0,
            0,
            0);

        // local reset
        TargetWeight = 0;
        TargetSets = 0;
        TargetReps = 0;

        await LoadAsync(ExerciseId);

        return targetDisplay;
    }

    // =========================
    // DELETE SET
    // =========================

    public async Task DeleteSetAsync(ExerciseSet set)
    {
        if (set == null)
            return;

        bool confirm = await _confirm("Möchtest du dieses Set wirklich löschen?");
        if (!confirm)
            return;

        await _databaseService.DeleteSetAsync(set.Id);

        Sets.Remove(set);
    }

    // =========================
    // REMOVE TARGET
    // =========================

    public async Task RemoveTargetAsync()
    {
        bool confirm = await _confirm("Möchtest du das Ziel entfernen?");
        if (!confirm)
            return;

        await _databaseService.SetExerciseTargetAsync(
            ExerciseId,
            0,
            0,
            0);

        TargetWeight = 0;
        TargetSets = 0;
        TargetReps = 0;

        await LoadAsync(ExerciseId);
    }
    public string TargetInfo
    {
        get
        {
            bool hasWeight = TargetWeight > 0;
            bool hasReps = TargetSets > 0 && TargetReps > 0;

            if (hasWeight && hasReps)
                return $"Ziel: {TargetWeight} kg · {TargetSets}x{TargetReps}";

            if (hasWeight)
                return $"Ziel: {TargetWeight} kg";

            if (hasReps)
                return $"Ziel: {TargetSets}x{TargetReps}";

            return "Kein Ziel gesetzt";
        }
    }
}