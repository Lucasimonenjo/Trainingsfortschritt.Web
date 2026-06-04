using Microsoft.JSInterop;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Trainingsfortschritt.Core.Models;
using Trainingsfortschritt.Core.Abstractions;

namespace Trainingsfortschritt.Web.ViewModels;

public class ProgressChartViewModel : INotifyPropertyChanged
{
    private readonly IDatabaseService _databaseService;
    private readonly IJSRuntime _js;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private bool _isLoaded;
    public bool IsLoaded
    {
        get => _isLoaded;
        set { _isLoaded = value; OnPropertyChanged(); }
    }

    private CancellationTokenSource? _cts;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public ObservableCollection<Exercise> Exercises { get; } = new();

    private List<ChartPoint> _chartPoints = new();
    public List<ChartPoint> ChartPoints
    {
        get => _chartPoints;
        set
        {
            _chartPoints = value;
            OnPropertyChanged();
        }
    }

    private int? _selectedExerciseId;
    public int? SelectedExerciseId
    {
        get => _selectedExerciseId;
        set
        {
            if (_selectedExerciseId == value) return;
            _selectedExerciseId = value;
            OnPropertyChanged();
        }
    }

    private string _selectedMetric = "Weight";
    public string SelectedMetric
    {
        get => _selectedMetric;
        set
        {
            if (_selectedMetric == value) return;
            _selectedMetric = value;
            OnPropertyChanged();
        }
    }

    private string _selectedRange = "30 Tage";
    public string SelectedRange
    {
        get => _selectedRange;
        set
        {
            if (_selectedRange == value) return;
            _selectedRange = value;
            OnPropertyChanged();
        }
    }

    public List<string> Metrics { get; } = new() { "Weight", "Reps" };
    public List<string> Ranges { get; } = new() { "7 Tage", "30 Tage", "Alle" };

    public ProgressChartViewModel(
        IDatabaseService databaseService,
        IJSRuntime js)
    {
        _databaseService = databaseService;
        _js = js;
    }

    // =========================
    // LOAD
    // =========================

    public async Task LoadAsync()
    {
        IsLoaded = false;

        var exercises = await _databaseService.GetExercisesAsync();

        Exercises.Clear();
        foreach (var ex in exercises)
            Exercises.Add(ex);

        SelectedMetric =
            await _js.InvokeAsync<string?>("localStorage.getItem", "Chart_SelectedMetric")
            ?? "Weight";

        SelectedRange =
            await _js.InvokeAsync<string?>("localStorage.getItem", "Chart_SelectedRange")
            ?? "30 Tage";

        var savedId =
            await _js.InvokeAsync<string?>("localStorage.getItem", "Chart_SelectedExerciseId");

        if (int.TryParse(savedId, out var id))
            SelectedExerciseId = id;

        IsLoaded = true;

        await RefreshChart();
    }

    // =========================
    // EXPLICIT REFRESH ONLY
    // =========================

    public async Task RefreshChart()
    {
        if (_selectedExerciseId is null || _selectedExerciseId <= 0)
            return;

        await _lock.WaitAsync();

        try
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            await LoadChart(_cts.Token);
        }
        finally
        {
            _lock.Release();
        }
    }

    // =========================
    // CHART
    // =========================

    private async Task LoadChart(CancellationToken ct)
    {
        var metric = SelectedMetric;
        var range = SelectedRange;
        var exerciseId = SelectedExerciseId!.Value;

        Console.WriteLine($"[Chart] START | Metric={metric} Range={range} ExerciseId={exerciseId}");

        var sets = await _databaseService.GetSetsForExerciseAsync(exerciseId);

        DateTime minDate = range switch
        {
            "7 Tage" => DateTime.Now.AddDays(-7),
            "30 Tage" => DateTime.Now.AddDays(-30),
            _ => DateTime.MinValue
        };

        var filtered = sets
            .Where(s => s.Date >= minDate)
            .OrderBy(s => s.Date)
            .ToList();

        var result = new List<ChartPoint>();

        int i = 0;

        foreach (var s in filtered)
        {
            ct.ThrowIfCancellationRequested();

            double value = metric switch
            {
                "Reps" => s.Reps,
                "Weight" => s.Weight,
                _ => s.Weight
            };

            Console.WriteLine($"[Chart] #{i++} {s.Date:HH:mm} W={s.Weight} R={s.Reps} -> {value}");

            result.Add(new ChartPoint
            {
                Date = s.Date,
                Value = value
            });
        }

        ChartPoints = result;

        Console.WriteLine($"[Chart] FINAL points: {ChartPoints.Count}");

        await _js.InvokeVoidAsync("localStorage.setItem", "Chart_SelectedMetric", metric);
        await _js.InvokeVoidAsync("localStorage.setItem", "Chart_SelectedRange", range);
        await _js.InvokeVoidAsync("localStorage.setItem", "Chart_SelectedExerciseId", exerciseId.ToString());
    }

    public class ChartPoint
    {
        public DateTime Date { get; set; }
        public double Value { get; set; }
    }
}