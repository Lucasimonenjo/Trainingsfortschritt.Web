using Microsoft.JSInterop;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Trainingsfortschritt.Core.Abstractions;
using Trainingsfortschritt.Core.Models;
using Trainingsfortschritt.Core.Services;
using Trainingsfortschritt.Web.Services;

namespace Trainingsfortschritt.Web.ViewModels;

public class AddSetViewModel : INotifyPropertyChanged
{
    private readonly IDatabaseService _databaseService;
    private readonly IJSRuntime _js;
    private readonly AchievementService _achievementService;
    private readonly AchievementPopupService _popupService;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private int _exerciseId;
    private double _weight;
    private string _repsInput = string.Empty;
    private string? _seatSetting;
    private string? _notes;

    private bool _isLoaded;
    private bool _isBusy;

    private string? _goalMessage;
    private bool _showTags = true;

    // =========================
    // PR DETECTION
    // =========================

    private bool _isPR;
    private double _prWeight;
    private string _prReps = string.Empty;

    public bool IsPR => _isPR;

    public double PRWeight => _prWeight;

    public string PRReps => _prReps;

    public AddSetViewModel(
    IDatabaseService databaseService,
    IJSRuntime js,
    AchievementService achievementService,
    AchievementPopupService popupService)
    {
        _databaseService = databaseService;
        _js = js;
        _achievementService = achievementService;
        _popupService = popupService;
    }

    // =========================
    // STATE
    // =========================

    public bool IsLoaded
    {
        get => _isLoaded;
        private set
        {
            _isLoaded = value;
            OnPropertyChanged();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            _isBusy = value;
            OnPropertyChanged();
        }
    }

    public int ExerciseId
    {
        get => _exerciseId;
        set
        {
            _exerciseId = value;
            _ = LoadAsync();
        }
    }

    public double Weight
    {
        get => _weight;
        set
        {
            _weight = value;
            OnPropertyChanged();
        }
    }

    public string RepsInput
    {
        get => _repsInput;
        set
        {
            _repsInput = value;
            OnPropertyChanged();
        }
    }

    public string? SeatSetting
    {
        get => _seatSetting;
        set
        {
            _seatSetting = value;
            OnPropertyChanged();
        }
    }

    public string? Notes
    {
        get => _notes;
        set
        {
            _notes = value;
            OnPropertyChanged();
        }
    }

    // =========================
    // TAGS
    // =========================

    public class TagItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _isSelected;

        public string Name { get; set; } = string.Empty;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;

                PropertyChanged?.Invoke(
                    this,
                    new PropertyChangedEventArgs(nameof(IsSelected))
                );
            }
        }
    }

    public ObservableCollection<TagItem> Tags { get; } = new()
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
    // TAG VISIBILITY
    // =========================

    public bool ShowTags
    {
        get => _showTags;
        private set
        {
            _showTags = value;
            OnPropertyChanged();
        }
    }

    // =========================
    // LOAD
    // =========================

    public async Task LoadAsync()
    {
        if (ExerciseId <= 0)
            return;

        IsLoaded = false;

        // =========================
        // SETTINGS LADEN
        // =========================

        var tagsTask = _js.InvokeAsync<string?>("localStorage.getItem", "tagsEnabled").AsTask();
        var seatTask = _js.InvokeAsync<string?>("localStorage.getItem", "saveSeatSettings").AsTask();
        var weightTask = _js.InvokeAsync<string?>("localStorage.getItem", "saveLastWeight").AsTask();
        var repsTask = _js.InvokeAsync<string?>("localStorage.getItem", "saveLastReps").AsTask();

        await Task.WhenAll(tagsTask, seatTask, weightTask, repsTask);

        bool showTags = tagsTask.Result != "false";
        bool saveSeat = seatTask.Result == "true";
        bool saveWeight = weightTask.Result == "true";
        bool saveReps = repsTask.Result == "true";

        // =========================
        // TAGS RESETTEN WENN AUS
        // =========================

        if (!ShowTags)
        {
            foreach (var tag in Tags)
                tag.IsSelected = false;
        }

        // =========================
        // LETZTES SET LADEN
        // =========================

        var last =
            await _databaseService.GetLatestSetForExerciseAsync(ExerciseId);

        if (last != null)
        {
            // Gewicht übernehmen
            if (saveWeight)
                Weight = last.Weight;

            // Wiederholungen übernehmen
            if (saveReps)
                RepsInput = last.RepsDisplay ?? string.Empty;

            // Sitzeinstellung übernehmen
            if (saveSeat)
                SeatSetting = last.SeatSetting;

            // Notizen bleiben immer (kein Setting nötig)
            Notes = last.Notes;
        }

        IsLoaded = true;
    }

    public Task RefreshAsync()
        => LoadAsync();

    // =========================
    // SAVE
    // =========================

    public async Task SaveAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;

        try
        {
            _isPR = false;

            int sets = 1;
            int repsPerSet = 0;

            if (!string.IsNullOrWhiteSpace(RepsInput)
                && RepsInput.Contains("x"))
            {
                var parts = RepsInput.Split('x');

                sets = int.Parse(parts[0].Trim());
                repsPerSet = int.Parse(parts[1].Trim());
            }
            else
            {
                repsPerSet =
                    int.TryParse(RepsInput, out var r)
                        ? r
                        : 0;
            }

            string? tags = null;

            if (ShowTags)
            {
                var selectedTags = Tags
                    .Where(t => t.IsSelected)
                    .Select(t => t.Name)
                    .ToList();

                if (selectedTags.Any())
                    tags = string.Join(",", selectedTags);
            }

            var set = new ExerciseSet
            {
                ExerciseId = ExerciseId,
                Weight = Weight,
                Reps = sets * repsPerSet,
                RepsDisplay = RepsInput,
                SeatSetting = SeatSetting,
                Notes = Notes,
                Date = DateTime.Now,
                Sets = sets,
                RepsPerSet = repsPerSet,
                Tags = tags
            };

            // =========================
            // PR DETECTION
            // =========================

            var prEnabled = await _js.InvokeAsync<string?>(
                "localStorage.getItem",
                "prDetectionEnabled"
            );

            bool isPREnabled = prEnabled == "true";

            if (isPREnabled)
            {
                var last =
                    await _databaseService.GetLatestSetForExerciseAsync(ExerciseId);

                if (last != null && Weight > last.Weight)
                {
                    _isPR = true;
                    _prWeight = Weight;
                    _prReps = RepsInput;

                    await _js.InvokeVoidAsync(
                        "sessionStorage.setItem",
                        "prPopup",
                        $"{_prWeight}|{_prReps}"
                    );
                }
            }

            // =========================
            // SAVE SET
            // =========================

            await _databaseService.AddSetAsync(set);

            Console.WriteLine("🔥 VIEWMODEL: ACH SERVICE CALL");

            var allSets = await _databaseService.GetAllSetsAsync();
            var exercises = await _databaseService.GetExercisesAsync();

            // =========================
            // ACHIEVEMENTS (NEW SYSTEM)
            // =========================

            var unlockedAchievements =
                await _achievementService.ProcessSets(allSets, exercises);

            Console.WriteLine($"🏆 RETURNED ACHIEVEMENTS: {unlockedAchievements.Count}");

            foreach (var ach in unlockedAchievements)
            {
                Console.WriteLine($"🔥 POPUP: {ach.Title}");
                _popupService.Queue(ach);
            }

            // ❌ REMOVE THIS:
            // await _popupService.ShowAsync();
           

            await _popupService.ShowAsync();

            Console.WriteLine("🔥 VIEWMODEL: ProcessSets fertig");

            // =========================
            // GOAL MESSAGE
            // =========================

            if (_databaseService is WebDatabaseService webDb)
            {
                _goalMessage =
                    await webDb.CheckGoalReachedAsync(set);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    // =========================
    // GOAL MESSAGE
    // =========================

    public Task<string?> GetGoalMessageAsync()
        => Task.FromResult(_goalMessage);

    // =========================
    // TAG TOGGLE
    // =========================

    public void ToggleTag(TagItem tag)
    {
        if (!ShowTags)
            return;

        if (tag == null)
            return;

        tag.IsSelected = !tag.IsSelected;
    }
}