using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.JSInterop;
using Trainingsfortschritt.Core.Models;
using Trainingsfortschritt.Core.Abstractions;

namespace Trainingsfortschritt.Web.ViewModels;

public class PersonalRecordsViewModel : INotifyPropertyChanged
{
    private readonly IDatabaseService _db;
    private readonly IJSRuntime _js;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // =========================
    // STATE
    // =========================

    private bool _isLoaded;
    public bool IsLoaded
    {
        get => _isLoaded;
        set { _isLoaded = value; OnPropertyChanged(); }
    }

    // =========================
    // DATA
    // =========================

    public ObservableCollection<Exercise> Exercises { get; } = new();
    public ObservableCollection<ExerciseStatRecord> Records { get; } = new();

    private int? _selectedExerciseId;

    public int? SelectedExerciseId
    {
        get => _selectedExerciseId;
        private set
        {
            if (_selectedExerciseId == value)
                return;

            _selectedExerciseId = value;
            OnPropertyChanged();
        }
    }

    // =========================
    // CONSTRUCTOR
    // =========================

    public PersonalRecordsViewModel(
        IDatabaseService db,
        IJSRuntime js)
    {
        _db = db;
        _js = js;
    }

    // =========================
    // LOAD
    // =========================

    public async Task LoadAsync()
    {
        IsLoaded = false;

        var exercises = await _db.GetExercisesAsync();

        Exercises.Clear();
        foreach (var ex in exercises)
            Exercises.Add(ex);

        var savedId =
            await _js.InvokeAsync<string?>("localStorage.getItem", "PR_SelectedExerciseId");

        if (int.TryParse(savedId, out var id))
        {
            SelectedExerciseId = id;
            await LoadRecordsAsync();
        }

        IsLoaded = true;
    }

    // =========================
    // EXTERNAL SET (SAFE FIX)
    // =========================

    public async Task SetExerciseAsync(int id)
    {
        SelectedExerciseId = id;

        await _js.InvokeVoidAsync(
            "localStorage.setItem",
            "PR_SelectedExerciseId",
            id.ToString()
        );

        await LoadRecordsAsync();
    }

    // =========================
    // RECORDS
    // =========================

    private async Task LoadRecordsAsync()
    {
        if (SelectedExerciseId is null || SelectedExerciseId <= 0)
            return;

        var records =
            await _db.GetExerciseStatRecordsAsync(SelectedExerciseId.Value);

        Records.Clear();

        foreach (var r in records)
            Records.Add(r);

        OnPropertyChanged(nameof(Records));
    }
}