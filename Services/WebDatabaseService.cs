using Microsoft.JSInterop;
using System.Text.Json;
using Trainingsfortschritt.Core.Abstractions;
using Trainingsfortschritt.Core.Models;
using Trainingsfortschritt.Core.Services;
using GoalHistoryModel = Trainingsfortschritt.Core.Models.GoalHistory;

namespace Trainingsfortschritt.Web.Services;

public class WebDatabaseService : IDatabaseService
{
    private readonly IndexedDbService _db;
    private readonly IJSRuntime _js;
    private readonly AchievementService _achievementService;
    private readonly IStorageService _storage;

    public WebDatabaseService(
    IndexedDbService db,
    IJSRuntime js,
    AchievementService achievementService,
    IStorageService storage)
    {
        _db = db;
        _js = js;
        _achievementService = achievementService;
        _storage = storage;
    }

    // =========================
    // EXERCISES
    // =========================

    public Task<List<Exercise>> GetExercisesAsync()
        => _db.GetExercisesAsync();

    public Task<Exercise?> GetExerciseByIdAsync(int id)
        => _db.GetExerciseByIdAsync(id);

    public async Task<bool> AddExerciseAsync(Exercise ex)
    {
        await _db.AddExerciseAsync(ex);
        return true;
    }

    public Task DeleteExerciseAsync(int id)
        => _db.DeleteExerciseAsync(id);

    public Task UpdateExerciseAsync(Exercise exercise)
        => _db.UpdateExerciseAsync(exercise);

    // =========================
    // SETS
    // =========================

    public Task<List<ExerciseSet>> GetAllSetsAsync()
        => _db.GetSetsAsync();

    public Task<List<ExerciseSet>> GetSetsForExerciseAsync(int exerciseId)
        => _db.GetSetsForExerciseAsync(exerciseId);

    public Task AddSetAsync(ExerciseSet set)
        => _db.AddSetAsync(set);

    public Task UpdateSetAsync(ExerciseSet set)
        => _db.UpdateSetAsync(set);

    public Task<ExerciseSet?> GetLatestSetForExerciseAsync(int exerciseId)
        => _db.GetLatestSetForExerciseAsync(exerciseId);

    public Task DeleteSetAsync(int setId)
        => _db.DeleteSetAsync(setId);

    // =========================
    // GOAL CHECK + SAVE
    // =========================

    public async Task<string?> CheckGoalReachedAsync(ExerciseSet newSet)
    {
        var exercise = await GetExerciseByIdAsync(newSet.ExerciseId);

        if (exercise == null)
            return null;

        double targetWeight = exercise.TargetWeight;
        int targetReps = exercise.TargetReps;

        bool hasWeightTarget = targetWeight > 0;
        bool hasRepsTarget = targetReps > 0;

        if (!hasWeightTarget && !hasRepsTarget)
            return null;

        bool reached =
            (hasWeightTarget ? newSet.Weight >= targetWeight : true) &&
            (hasRepsTarget ? newSet.Reps >= targetReps : true);

        if (!reached)
            return null;

        // =========================
        // ALLE SETS LADEN
        // =========================
        var sets = await GetSetsForExerciseAsync(exercise.Id);

        Console.WriteLine("==================================");
        Console.WriteLine($"EXERCISE: {exercise.Name}");
        Console.WriteLine($"SETS TOTAL: {sets.Count}");

        foreach (var s in sets)
        {
            Console.WriteLine($"SET -> Date: {s.Date}, Weight: {s.Weight}, Reps: {s.Reps}");
        }

        // =========================
        // STARTZEIT (FIXED LOGIK)
        // =========================
        DateTime startDate =
            exercise.TargetSetDate
            ?? sets.OrderBy(s => s.Date).FirstOrDefault()?.Date
            ?? newSet.Date;

        Console.WriteLine($"START DATE: {startDate}");

        // =========================
        // FILTER SETS
        // =========================
        var startDateNormalized = startDate.Date;

        var setsSinceStart = sets
            .Where(s => s.Date.Date >= startDateNormalized)
            .ToList();

        Console.WriteLine($"SETS SINCE START: {setsSinceStart.Count}");

        foreach (var s in setsSinceStart)
        {
            Console.WriteLine($"ACTIVE SET -> {s.Date}");
        }

        // =========================
        // TRAININGSEINHEITEN = SETS
        // =========================
        int workoutsNeeded = setsSinceStart.Count;

        Console.WriteLine($"WORKOUTS (SETS): {workoutsNeeded}");

        // =========================
        // TAGE (mindestens 1)
        // =========================
        int daysNeeded =
            Math.Max((DateTime.Now.Date - startDate.Date).Days + 1, 1);

        Console.WriteLine($"DAYS NEEDED: {daysNeeded}");
        Console.WriteLine("==================================");

        // =========================
        // GOAL SPEICHERN
        // =========================
        var goal = new GoalHistory
        {
            ExerciseId = exercise.Id,
            ExerciseName = exercise.Name,

            TargetWeight = targetWeight,
            TargetSets = exercise.TargetSets,
            TargetReps = exercise.TargetReps,

            TargetRepsDisplay =
                hasRepsTarget
                    ? $"{exercise.TargetSets}x{exercise.TargetReps}"
                    : null,

            DateReached = DateTime.Now,

            DaysNeeded = daysNeeded,
            WorkoutsNeeded = workoutsNeeded
        };

        await AddGoalHistoryAsync(goal);

        // =========================
        // RESET
        // =========================
        exercise.TargetWeight = 0;
        exercise.TargetSets = 0;
        exercise.TargetReps = 0;
        exercise.TargetSetDate = null;

        await UpdateExerciseAsync(exercise);

        return hasWeightTarget
            ? $"{targetWeight} kg"
            : "Ziel erreicht";
    }

    // =========================
    // GOALS (FIXED)
    // =========================

    public Task AddGoalHistoryAsync(GoalHistory g)
    => _db.AddGoalAsync(g);

    public Task<List<GoalHistory>> GetGoalHistoryAsync()
        => _db.GetGoalsAsync();

    public Task ClearGoalHistoryAsync()
        => _db.ClearGoalHistoryAsync();

    // =========================
    // EXPORT / IMPORT
    // =========================

    public async Task<string> ExportDatabaseAsync()
    {
        Console.WriteLine("🔥 WEBDATABASE EXPORT");
        var exercises = await _db.GetExercisesAsync();
        var sets = await _db.GetSetsAsync();
        var goals = await _db.GetGoalsAsync();

        Console.WriteLine("========== EXPORT START ==========");
        Console.WriteLine($"Exercises: {exercises.Count}");
        Console.WriteLine($"Sets: {sets.Count}");
        Console.WriteLine($"Goals: {goals.Count}");

        var achievements = new List<object>();

        int loadedCount = 0;
        int unlockedCount = 0;

        foreach (var a in _achievementService.Achievements)
        {
            var value = await _storage.GetDoubleAsync($"ach_{a.Id}_value", 0);
            var unlocked = await _storage.GetBoolAsync($"ach_{a.Id}_unlocked", false);
            var ticks = await _storage.GetDoubleAsync($"ach_{a.Id}_date", 0);

            Console.WriteLine($"[ACH EXPORT] {a.Id}");
            Console.WriteLine($"   value = {value}");
            Console.WriteLine($"   unlocked = {unlocked}");
            Console.WriteLine($"   ticks = {ticks}");

            if (value > 0 || unlocked)
                loadedCount++;

            if (unlocked)
                unlockedCount++;

            achievements.Add(new
            {
                a.Id,
                CurrentValue = value,
                IsUnlocked = unlocked,
                UnlockDateTicks = ticks
            });
        }

        Console.WriteLine("========== EXPORT SUMMARY ==========");
        Console.WriteLine($"Achievements total: {_achievementService.Achievements.Count}");
        Console.WriteLine($"Achievements exported: {achievements.Count}");
        Console.WriteLine($"Achievements with data: {loadedCount}");
        Console.WriteLine($"Unlocked achievements: {unlockedCount}");
        Console.WriteLine("===================================");

        var exportObject = new
        {
            ExportDate = DateTime.UtcNow,
            Exercises = exercises,
            Sets = sets,
            GoalHistory = goals,
            Achievements = achievements
        };

        var json = JsonSerializer.Serialize(exportObject, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        Console.WriteLine($"EXPORT SIZE: {json.Length} chars");
        Console.WriteLine("========== EXPORT END ==========");

        return json;
    }

    public async Task ImportDatabaseFromJsonAsync(string json)
    {
        await _db.ImportAsync(json);

        // 🔥 danach ACHIEVEMENT SYSTEM SYNC
        await _achievementService.Initialize();
    }

    public Task ImportDatabaseAsync()
        => Task.CompletedTask;

    // =========================
    // TARGETS
    // =========================

    public async Task SetExerciseTargetAsync(
    int exerciseId,
    double weight,
    int sets,
    int reps)
    {
        var exercise = await GetExerciseByIdAsync(exerciseId);

        if (exercise == null)
            return;

        exercise.TargetWeight = weight;
        exercise.TargetSets = sets;
        exercise.TargetReps = reps;

        exercise.TargetSetDate = DateTime.Now; // <-- WICHTIG FÜR DAYS/WORKOUTS

        await UpdateExerciseAsync(exercise);
    }

    // =========================
    // RESET
    // =========================

    public Task ResetDatabaseAsync()
        => _db.ClearAllAsync();

    public Task ClearAllAsync()
        => _db.ClearAllAsync();

    // =========================
    // STATS
    // =========================

    public async Task<List<ExerciseStatRecord>> GetExerciseStatRecordsAsync(int exerciseId)
    {
        var sets = await GetSetsForExerciseAsync(exerciseId);

        if (sets == null || sets.Count == 0)
            return new List<ExerciseStatRecord>();

        var result = new List<ExerciseStatRecord>();

        var maxWeightSet = sets.OrderByDescending(s => s.Weight).FirstOrDefault();
        if (maxWeightSet != null)
        {
            result.Add(new ExerciseStatRecord
            {
                Title = "Max Gewicht",
                Value = maxWeightSet.Weight,
                Date = maxWeightSet.Date
            });
        }

        var bestRepsSet = sets.OrderByDescending(s => s.Reps).FirstOrDefault();
        if (bestRepsSet != null)
        {
            result.Add(new ExerciseStatRecord
            {
                Title = "Beste Wiederholungen",
                Value = bestRepsSet.Reps,
                Date = bestRepsSet.Date
            });
        }

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
    // INTERNAL HELPERS
    // =========================

    private async Task<T?> GetAsync<T>(string key)
    {
        var json = await _js.InvokeAsync<string>("idbStorage.get", key);

        if (string.IsNullOrWhiteSpace(json))
            return default;

        return JsonSerializer.Deserialize<T>(json);
    }
    public Task<List<GoalHistory>> GetGoalsAsync()
    => _db.GetGoalsAsync();
}