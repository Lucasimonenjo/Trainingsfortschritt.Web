using System.Text.Json;
using Microsoft.JSInterop;
using Trainingsfortschritt.Core.Models;
using Trainingsfortschritt.Web.Models;

namespace Trainingsfortschritt.Web.Services;

public class IndexedDbService
{
    private readonly IJSRuntime _js;

    public IndexedDbService(IJSRuntime js)
    {
        _js = js;
    }

    // =========================
    // LOW LEVEL JS
    // =========================

    private async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var json = await _js.InvokeAsync<string>("idbStorage.get", key);

            if (string.IsNullOrWhiteSpace(json))
                return default;

            return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }

    private Task SetAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);

        return _js.InvokeVoidAsync("idbStorage.set", key, json).AsTask();
    }

    private Task DeleteAsync(string key)
        => _js.InvokeVoidAsync("idbStorage.remove", key).AsTask();

    private Task ClearAsync()
        => _js.InvokeVoidAsync("idbStorage.clear").AsTask();

    // =========================
    // KEYS
    // =========================

    private const string EX = "exercises";
    private const string SE = "sets";
    private const string GO = "goals";

    // =========================
    // EXERCISES
    // =========================

    public async Task<List<Exercise>> GetExercisesAsync()
        => await GetAsync<List<Exercise>>(EX) ?? new();

    public async Task AddExerciseAsync(Exercise ex)
    {
        var list = await GetExercisesAsync();

        ex.Id = list.Count == 0 ? 1 : list.Max(x => x.Id) + 1;

        list.Add(ex);

        await SetAsync(EX, list);
    }

    public async Task DeleteExerciseAsync(int id)
    {
        var exercises = await GetExercisesAsync();
        var sets = await GetSetsAsync();

        exercises.RemoveAll(x => x.Id == id);
        sets.RemoveAll(x => x.ExerciseId == id);

        await SetAsync(EX, exercises);
        await SetAsync(SE, sets);
    }

    public async Task<Exercise?> GetExerciseByIdAsync(int id)
        => (await GetExercisesAsync()).FirstOrDefault(x => x.Id == id);

    public async Task UpdateExerciseAsync(Exercise exercise)
    {
        var exercises = await GetExercisesAsync();

        var existing = exercises.FirstOrDefault(x => x.Id == exercise.Id);
        if (existing == null)
            return;

        existing.Name = exercise.Name;
        existing.TargetWeight = exercise.TargetWeight;
        existing.TargetSets = exercise.TargetSets;
        existing.TargetReps = exercise.TargetReps;
        existing.TargetSetDate = exercise.TargetSetDate;
        existing.GoalsReachedCount = exercise.GoalsReachedCount;

        await SetAsync(EX, exercises);
    }

    // =========================
    // SETS
    // =========================

    public async Task<List<ExerciseSet>> GetSetsAsync()
        => await GetAsync<List<ExerciseSet>>(SE) ?? new();

    public async Task AddSetAsync(ExerciseSet set)
    {
        var list = await GetSetsAsync();

        set.Id = list.Count == 0 ? 1 : list.Max(x => x.Id) + 1;

        // 🔥 FIX: stabile Zeit + eindeutigkeit
        set.Date = DateTime.UtcNow.AddTicks(Environment.TickCount % 10000);

        list.Add(set);

        await SetAsync(SE, list);
    }

    public async Task<List<ExerciseSet>> GetSetsForExerciseAsync(int exerciseId)
    {
        var sets = await GetSetsAsync();

        // 🔥 CRITICAL FIX: stabile Sortierung
        return sets
            .Where(x => x.ExerciseId == exerciseId)
            .OrderBy(x => x.Date)
            .ThenBy(x => x.Id)
            .ToList();
    }

    public async Task<ExerciseSet?> GetLatestSetForExerciseAsync(int exerciseId)
    {
        var sets = await GetSetsAsync();

        return sets
            .Where(x => x.ExerciseId == exerciseId)
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.Id)
            .FirstOrDefault();
    }

    public async Task DeleteSetAsync(int setId)
    {
        var sets = await GetSetsAsync();

        sets.RemoveAll(x => x.Id == setId);

        await SetAsync(SE, sets);
    }

    // =========================
    // GOALS
    // =========================

    public async Task ClearGoalHistoryAsync()
        => await SetAsync(GO, new List<GoalHistory>());

    // =========================
    // RESET
    // =========================

    public async Task ClearAllAsync()
    {
        await SetAsync(EX, new List<Exercise>());
        await SetAsync(SE, new List<ExerciseSet>());
        await SetAsync(GO, new List<GoalHistory>());
    }

    // =========================
    // EXPORT / IMPORT
    // =========================

    public async Task<string> ExportAsync()
    {
        Console.WriteLine("🔥 INDEXEDDB EXPORT");
        var data = new
        {
            Exercises = await GetExercisesAsync(),
            Sets = await GetSetsAsync(),
            Goals = await GetGoalsAsync()
        };

        return JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    public async Task ImportAsync(string json)
    {
        Console.WriteLine("🔥 IMPORTASYNC CALLED");

        var data = JsonSerializer.Deserialize<ImportModel>(json);

        if (data == null)
            return;

        if (data.Exercises != null)
            await SetAsync(EX, data.Exercises);

        if (data.Sets != null)
            await SetAsync(SE, data.Sets);

        if (data.Goals != null)
            await SetAsync(GO, data.Goals);

        // ❗ ACHIEVEMENTS MISSING FIX
        if (data.Achievements != null)
        {
            foreach (var a in data.Achievements)
            {
                await _js.InvokeVoidAsync("idbStorage.set", $"ach_{a.Id}_value", a.CurrentValue);
                await _js.InvokeVoidAsync("idbStorage.set", $"ach_{a.Id}_unlocked", a.IsUnlocked);
                await _js.InvokeVoidAsync("idbStorage.set", $"ach_{a.Id}_date", a.UnlockDateTicks);
            }
        }
    }

    private class ImportModel
    {
        public List<Exercise>? Exercises { get; set; }
        public List<ExerciseSet>? Sets { get; set; }
        public List<GoalHistory>? Goals { get; set; }
        public List<AchievementImport>? Achievements { get; set; }
    }
    private class AchievementImport
    {
        public string Id { get; set; } = string.Empty;
        public double CurrentValue { get; set; }
        public bool IsUnlocked { get; set; }
        public double UnlockDateTicks { get; set; }
    }

    public async Task<ExerciseSet?> GetSetByIdAsync(int id)
    {
        var sets = await GetSetsAsync();
        return sets.FirstOrDefault(x => x.Id == id);
    }

    public async Task UpdateSetAsync(ExerciseSet updated)
    {
        var sets = await GetSetsAsync();

        var existing = sets.FirstOrDefault(x => x.Id == updated.Id);
        if (existing == null)
            return;

        existing.Weight = updated.Weight;
        existing.Reps = updated.Reps;
        existing.RepsDisplay = updated.RepsDisplay;
        existing.Date = updated.Date;
        existing.Notes = updated.Notes;
        existing.Tags = updated.Tags;
        existing.SeatSetting = updated.SeatSetting;

        await SetAsync(SE, sets);
    }

    public async Task<string?> CheckGoalReachedAsync(ExerciseSet newSet)
    {
        var exercise = await GetExerciseByIdAsync(newSet.ExerciseId);

        if (exercise == null)
            return null;

        double targetWeight = exercise.TargetWeight;
        int targetReps = exercise.TargetReps;

        bool hasWeight = targetWeight > 0;
        bool hasReps = targetReps > 0;

        if (!hasWeight && !hasReps)
            return null;

        bool reached =
            (hasWeight ? newSet.Weight >= targetWeight : true) &&
            (hasReps ? newSet.Reps >= targetReps : true);

        if (!reached)
            return null;

        string targetDisplay =
            hasReps
                ? $"{targetWeight} kg · {targetReps}x"
                : $"{targetWeight} kg";

        exercise.TargetWeight = 0;
        exercise.TargetSets = 0;
        exercise.TargetReps = 0;

        await UpdateExerciseAsync(exercise);

        return targetDisplay;
    }

    public async Task FixSwappedRepsAndWeightAsync()
    {
        var sets = await GetSetsAsync();

        foreach (var s in sets)
        {
            double realWeight = s.Reps;
            int realReps = (int)s.Weight;

            s.Weight = realWeight;
            s.Reps = realReps;
        }

        await SetAsync(SE, sets);
    }
    public async Task<List<ExerciseStatRecord>> GetExerciseStatRecordsAsync(int exerciseId)
    {
        var sets = await GetSetsForExerciseAsync(exerciseId);

        if (sets == null || sets.Count == 0)
            return new List<ExerciseStatRecord>();

        var result = new List<ExerciseStatRecord>();

        // =========================
        // MAX WEIGHT
        // =========================
        var maxWeightSet = sets
            .OrderByDescending(s => s.Weight)
            .FirstOrDefault();

        if (maxWeightSet != null)
        {
            result.Add(new ExerciseStatRecord
            {
                Title = "Max Gewicht",
                Value = maxWeightSet.Weight,
                Date = maxWeightSet.Date
            });
        }

        // =========================
        // BEST REPS
        // =========================
        var bestRepsSet = sets
            .OrderByDescending(s => s.Reps)
            .FirstOrDefault();

        if (bestRepsSet != null)
        {
            result.Add(new ExerciseStatRecord
            {
                Title = "Beste Wiederholungen",
                Value = bestRepsSet.Reps,
                Date = bestRepsSet.Date
            });
        }

        // =========================
        // BEST 1RM (Epley)
        // =========================
        var best1RM = sets
            .Select(s => new
            {
                Set = s,
                OneRM = s.Weight * (1 + s.Reps / 30.0)
            })
            .OrderByDescending(x => x.OneRM)
            .FirstOrDefault();

        if (best1RM != null)
        {
            result.Add(new ExerciseStatRecord
            {
                Title = "Bester 1RM",
                Value = Math.Round(best1RM.OneRM, 1),
                Date = best1RM.Set.Date
            });
        }

        return result;
    }
    // =========================
    // GOALS
    // =========================

    public async Task<List<GoalHistory>> GetGoalsAsync()
    {
        var json = await _js.InvokeAsync<string>("idbStorage.get", "goals");

        if (string.IsNullOrWhiteSpace(json))
            return new List<GoalHistory>();

        return JsonSerializer.Deserialize<List<GoalHistory>>(json)
               ?? new List<GoalHistory>();
    }
    public async Task AddGoalAsync(GoalHistory goal)
    {
        var list = await GetGoalsAsync();

        goal.Id = list.Count == 0 ? 1 : list.Max(x => x.Id) + 1;

        list.Add(goal);

        await SetAsync("goals", list);
    }

    public async Task ClearGoalsAsync()
    {
        await SetAsync("goals", new List<GoalHistory>());
    }

    // helper
    private async Task AddToListAsync<T>(string key, T item)
    {
        var list = await GetAsync<List<T>>(key) ?? new List<T>();
        list.Add(item);
        await SetAsync(key, list);
    }
}