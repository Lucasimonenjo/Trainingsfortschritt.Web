using System.Collections.ObjectModel;
using Trainingsfortschritt.Core.Abstractions;
using Trainingsfortschritt.Core.Models;

namespace Trainingsfortschritt.Web.ViewModels;

public class StatisticsDashboardViewModel
{
    private readonly IDatabaseService _db;

    public bool IsLoaded { get; set; }

    public int WorkoutsThisWeek { get; set; }
    public int WorkoutsThisMonth { get; set; }

    public ObservableCollection<KeyValuePair<string, int>> MuscleDistribution { get; set; }
        = new();

    public StatisticsDashboardViewModel(IDatabaseService db)
    {
        _db = db;
    }

    public async Task LoadAsync()
    {
        IsLoaded = false;

        var sets = await _db.GetAllSetsAsync();
        var exercises = await _db.GetExercisesAsync();

        var today = DateTime.Now.Date;

        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var monthStart = new DateTime(today.Year, today.Month, 1);

        // =========================
        // WEEK / MONTH WORKOUTS
        // =========================

        WorkoutsThisWeek = sets
            .Where(s => s.Date.Date >= weekStart)
            .Select(s => s.Date.Date)
            .Distinct()
            .Count();

        WorkoutsThisMonth = sets
            .Where(s => s.Date.Date >= monthStart)
            .Select(s => s.Date.Date)
            .Distinct()
            .Count();

        // =========================
        // EXERCISE FREQUENCY (FIXED)
        // =========================

        var grouped = sets
            .GroupBy(s => s.ExerciseId) // 🔥 FIX: KEIN Convert.ToInt64
            .Select(g =>
            {
                var name = exercises.FirstOrDefault(e => e.Id == g.Key)?.Name ?? "Unknown";
                return new KeyValuePair<string, int>(name, g.Count());
            })
            .OrderByDescending(x => x.Value)
            .ToList();

        MuscleDistribution.Clear();

        foreach (var item in grouped)
            MuscleDistribution.Add(item);

        IsLoaded = true;
    }
}