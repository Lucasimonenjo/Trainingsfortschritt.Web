using System.Collections.ObjectModel;
using Trainingsfortschritt.Core.Abstractions;
using Trainingsfortschritt.Web.Models;

namespace Trainingsfortschritt.Web.ViewModels;

public class GoalHistoryViewModel
{
    private readonly IDatabaseService _db;

    public bool IsLoaded { get; set; }

    public ObservableCollection<GoalHistoryDisplayItem> History { get; } = new();
    public ObservableCollection<GoalHistoryDisplayItem> FilteredHistory { get; } = new();

    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value)
                return;

            _searchText = value;
            ApplyFilter();
        }
    }

    public GoalHistoryViewModel(IDatabaseService db)
    {
        _db = db;
    }

    // =========================
    // LOAD
    // =========================
    public async Task LoadAsync()
    {
        IsLoaded = false;

        var items = await _db.GetGoalsAsync();

        History.Clear();
        FilteredHistory.Clear();

        foreach (var h in items)
        {
            var item = new GoalHistoryDisplayItem
            {
                ExerciseName = h.ExerciseName,
                TargetWeight = h.TargetWeight,

                Sets = h.TargetSets,
                Reps = h.TargetReps ?? 0,

                TargetRepsDisplay = h.TargetRepsDisplay,

                DateReached = h.DateReached,
                DaysNeeded = h.DaysNeeded,
                WorkoutsNeeded = h.WorkoutsNeeded
            };

            History.Add(item);
            FilteredHistory.Add(item);
        }

        IsLoaded = true;
    }

    // =========================
    // FILTER
    // =========================
    public void ApplyFilter()
    {
        FilteredHistory.Clear();

        foreach (var item in History)
        {
            if (string.IsNullOrWhiteSpace(SearchText) ||
                item.ExerciseName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            {
                FilteredHistory.Add(item);
            }
        }
    }

    // =========================
    // CLEAR
    // =========================
    public async Task ClearAsync()
    {
        await _db.ClearGoalHistoryAsync();
        await LoadAsync();
    }
}