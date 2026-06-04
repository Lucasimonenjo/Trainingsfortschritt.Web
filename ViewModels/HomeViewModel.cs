using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Trainingsfortschritt.Core.Abstractions;
using Trainingsfortschritt.Core.Helpers;
using Trainingsfortschritt.Core.Models;
using Trainingsfortschritt.Web.Services;

namespace Trainingsfortschritt.Web.ViewModels;

public class HomeViewModel : IDisposable
{
    private readonly IDatabaseService _databaseService;
    private readonly Func<string, Task<bool>> _confirm;
    private readonly IJSRuntime _js;
    private readonly SettingsViewModel _settings;

    private readonly Dictionary<string, bool> _groupState = new();

    private CancellationTokenSource? _refreshCts;
    private bool _isLoading;

    public HomeViewModel(
        IDatabaseService databaseService,
        Func<string, Task<bool>> confirm,
        IJSRuntime js,
        SettingsViewModel settings)
    {
        _databaseService = databaseService;
        _confirm = confirm;
        _js = js;
        _settings = settings;
    }

    // =========================
    // STATE
    // =========================

    public List<ExerciseOverviewItem> Exercises { get; private set; } = new();

    public List<ExerciseGroup> GroupedExercises { get; private set; } = new();

    public bool IsLoaded { get; private set; }

    // =========================
    // LOAD
    // =========================

    public async Task LoadAsync()
    {
        var goals = await _databaseService.GetGoalHistoryAsync();
        await _settings.LoadAsync(); // 🔥 WICHTIG

        if (_isLoading)
            return;

        _isLoading = true;

        try
        {
            var exercises = await _databaseService.GetExercisesAsync();

            var items = new List<ExerciseOverviewItem>();

            foreach (var ex in exercises)
            {
                var latest =
                    await _databaseService.GetLatestSetForExerciseAsync(ex.Id);

                items.Add(new ExerciseOverviewItem
                {
                    ExerciseId = ex.Id,
                    Name = ex.Name,

                    Weight = latest?.Weight,
                    Reps = latest?.Reps,
                    RepsDisplay = latest?.RepsDisplay,
                    Date = latest?.Date,

                    TargetWeight = ex.TargetWeight,
                    TargetSets = ex.TargetSets > 0 ? ex.TargetSets : 1,
                    TargetReps = ex.TargetReps > 0 ? ex.TargetReps : 1,

                    GoalsReachedCount = goals.Count(g => g.ExerciseId == ex.Id),

                    ProgressPercent = CalculateProgress(latest, ex)
                });
            }

            Exercises = items;

            SyncGroupState(items);

            // 🔥 GROUP LOGIC CONTROLLED BY SETTING
            if (_settings.GroupExercises)
            {
                BuildGroups(items);
            }
            else
            {
                GroupedExercises = new List<ExerciseGroup>
                {
                    new ExerciseGroup("Alle Übungen", items)
                    {
                        IsExpanded = true
                    }
                };
            }

            if (!IsLoaded)
                StartAutoRefresh();

            IsLoaded = true;
        }
        finally
        {
            _isLoading = false;
        }
    }

    // =========================
    // AUTO REFRESH
    // =========================

    public void StartAutoRefresh()
    {
        _refreshCts?.Cancel();
        _refreshCts = new CancellationTokenSource();

        _ = Task.Run(async () =>
        {
            while (!_refreshCts.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), _refreshCts.Token);

                    if (!_refreshCts.Token.IsCancellationRequested)
                        await LoadAsync();
                }
                catch
                {
                    break;
                }
            }
        });
    }

    public void StopAutoRefresh()
    {
        _refreshCts?.Cancel();
        _refreshCts = null;
    }

    // =========================
    // GROUPING
    // =========================

    private void BuildGroups(List<ExerciseOverviewItem> items)
    {
        GroupedExercises.Clear();

        var groups = items
            .GroupBy(x => ExerciseData.GetFamily(x.Name))
            .OrderBy(g => g.Key);

        foreach (var g in groups)
        {
            string groupName = g.Key;

            var list = g.OrderBy(x => x.Name).ToList();

            GroupedExercises.Add(new ExerciseGroup(groupName, list)
            {
                IsExpanded =
                    _groupState.TryGetValue(groupName, out var state)
                        ? state
                        : false
            });
        }
    }

    // =========================
    // DELETE
    // =========================

    public async Task DeleteExerciseAsync(int id)
    {
        var exercise = Exercises.FirstOrDefault(x => x.ExerciseId == id);

        if (exercise == null)
            return;

        bool confirm =
            await _confirm($"Möchtest du die Übung „{exercise.Name}“ wirklich löschen?");

        if (!confirm)
            return;

        string groupName = ExerciseData.GetFamily(exercise.Name);

        await _databaseService.DeleteExerciseAsync(id);

        Exercises.RemoveAll(x => x.ExerciseId == id);

        var group = GroupedExercises.FirstOrDefault(x => x.Name == groupName);

        if (group != null)
        {
            var item = group.FirstOrDefault(x => x.ExerciseId == id);
            if (item != null)
                group.Remove(item);

            _groupState[groupName] = group.IsExpanded;
        }

        BuildGroups(Exercises);
    }

    // =========================
    // RESET
    // =========================

    public async Task ResetDatabaseAsync()
    {
        await _databaseService.ResetDatabaseAsync();
        await LoadAsync();
    }

    // =========================
    // TOGGLE GROUP
    // =========================

    public void ToggleGroup(string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
            return;

        bool current = _groupState.TryGetValue(groupName, out var state) && state;

        _groupState[groupName] = !current;

        var group = GroupedExercises.FirstOrDefault(x => x.Name == groupName);

        if (group != null)
            group.IsExpanded = !current;
    }

    // =========================
    // STATE SYNC
    // =========================

    private void SyncGroupState(List<ExerciseOverviewItem> items)
    {
        var groups = items
            .GroupBy(x => ExerciseData.GetFamily(x.Name))
            .Select(g => g.Key)
            .ToList();

        foreach (var g in groups)
            if (!_groupState.ContainsKey(g))
                _groupState[g] = false;

        foreach (var key in _groupState.Keys.ToList())
            if (!groups.Contains(key))
                _groupState.Remove(key);
    }

    // =========================
    // PROGRESS
    // =========================

    private double CalculateProgress(ExerciseSet? latest, Exercise? exercise)
    {
        if (latest == null || exercise == null)
            return 0;

        // Kein Ziel gesetzt
        if (exercise.TargetWeight <= 0 &&
            exercise.TargetReps <= 0 &&
            exercise.TargetSets <= 0)
            return 0;

        // =========================
        // WEIGHT RATIO
        // =========================
        double weightRatio = 1;

        if (exercise.TargetWeight > 0)
        {
            weightRatio =
                latest.Weight > 0
                    ? latest.Weight / exercise.TargetWeight
                    : 0;
        }

        // =========================
        // REPS RATIO (SETS x REPS)
        // =========================
        int targetSets = Math.Max(exercise.TargetSets, 1);
        int targetReps = Math.Max(exercise.TargetReps, 1);

        int actualSets = Math.Max(latest.Sets, 1);
        int actualReps = Math.Max(latest.Reps, 1);

        double targetTotal = targetSets * targetReps;
        double actualTotal = actualSets * actualReps;

        double repsRatio = actualTotal / targetTotal;

        // =========================
        // FINAL (wie MAUI: MIN statt Durchschnitt!)
        // =========================
        double ratio = Math.Min(weightRatio, repsRatio);

        return Math.Clamp(ratio * 100.0, 0, 100);
    }

    // =========================
    // DISPOSE
    // =========================

    public void Dispose()
    {
        StopAutoRefresh();
    }

    // =========================
    // STORAGE RESET
    // =========================

    public async Task ResetStorage()
    {
        await _databaseService.ClearAllAsync();
    }
    public Task OpenGoalHistoryAsync(NavigationManager nav)
    {
        nav.NavigateTo("/goal-history");
        return Task.CompletedTask;
    }
}