using System.Collections.ObjectModel;
using System.Linq;
using Trainingsfortschritt.Core.Abstractions;
using Trainingsfortschritt.Core.Helpers;
using Trainingsfortschritt.Core.Models;

namespace Trainingsfortschritt.Core.Services;

public class AchievementService
{
    private readonly IStorageService _storage;

    public ObservableCollection<AchievementItem> Achievements { get; } = new();

    public event Action? ProgressUpdated;
    public event Action<AchievementItem>? AchievementUnlocked;
    public event Action? AchievementsReset;
    private bool _initialized = false;
    private DateTime? _disabledSince = null;
    private DateTime? _systemEnabledSince;
    private const string SYSTEM_ENABLED_SINCE_KEY = "ach_system_since";

    public AchievementService(IStorageService storage)
    {
        Console.WriteLine("SERVICE INSTANCE: " + GetHashCode());
        _storage = storage;
    }


    private void Init()
    {
        Achievements.Clear();

        // =========================
        // COMMON
        // =========================

        Achievements.Add(new AchievementItem
        {
            Id = "first_step",
            Title = "First Step",
            Description = "Erstes Training abgeschlossen",
            Rarity = "COMMON",
            TargetValue = 1
        });

        Achievements.Add(new AchievementItem
        {
            Id = "warmup",
            Title = "Warm-Up",
            Description = "10 Trainingssessions",
            Rarity = "COMMON",
            TargetValue = 10
        });

        Achievements.Add(new AchievementItem
        {
            Id = "consistency_starter",
            Title = "Consistency Starter",
            Description = "3 Tage in Folge trainiert",
            Rarity = "COMMON",
            TargetValue = 3
        });

        Achievements.Add(new AchievementItem
        {
            Id = "rep_builder",
            Title = "Rep Builder",
            Description = "100 Wiederholungen insgesamt",
            Rarity = "COMMON",
            TargetValue = 100
        });

        Achievements.Add(new AchievementItem
        {
            Id = "set_collector",
            Title = "Set Collector",
            Description = "25 Sets insgesamt",
            Rarity = "COMMON",
            TargetValue = 25
        });

        // =========================
        // RARE
        // =========================

        Achievements.Add(new AchievementItem
        {
            Id = "hercules",
            Title = "Hercules",
            Description = "5000 kg Gesamtvolumen",
            Rarity = "RARE",
            TargetValue = 15000
        });

        Achievements.Add(new AchievementItem
        {
            Id = "superman",
            Title = "Superman",
            Description = "50 Trainingssessions",
            Rarity = "RARE",
            TargetValue = 50
        });

        Achievements.Add(new AchievementItem
        {
            Id = "baeren_stark",
            Title = "Bärenstark",
            Description = "100 kg+ in einer Übung erreicht",
            Rarity = "RARE",
            TargetValue = 100
        });

        Achievements.Add(new AchievementItem
        {
            Id = "break_limits",
            Title = "Break Your Limits",
            Description = "PR verbessert",
            Rarity = "RARE",
            TargetValue = 1
        });

        Achievements.Add(new AchievementItem
        {
            Id = "consistency_pro",
            Title = "Consistency Pro",
            Description = "10 Tage Training ohne Pause",
            Rarity = "RARE",
            TargetValue = 10
        });

        Achievements.Add(new AchievementItem
        {
            Id = "are_you_natty",
            Title = "Are you natty?!",
            Description = "Starke Leistungssteigerung",
            Rarity = "EPIC",
            TargetValue = 1
        });

        // =========================
        // EPIC
        // =========================

        Achievements.Add(new AchievementItem
        {
            Id = "hulk",
            Title = "Hulk",
            Description = "100.000 kg Gesamtvolumen",
            Rarity = "EPIC",
            TargetValue = 100000
        });

        Achievements.Add(new AchievementItem
        {
            Id = "aktienkurs",
            Title = "Aktienkurs",
            Description = "20 PRs über Zeit",
            Rarity = "EPIC",
            TargetValue = 20
        });

        Achievements.Add(new AchievementItem
        {
            Id = "iron_discipline",
            Title = "Iron Discipline",
            Description = "30 Tage Trainingsserie",
            Rarity = "EPIC",
            TargetValue = 30
        });

        Achievements.Add(new AchievementItem
        {
            Id = "machine_mode",
            Title = "Machine Mode",
            Description = "200 Trainingssessions",
            Rarity = "EPIC",
            TargetValue = 200
        });

        Achievements.Add(new AchievementItem
        {
            Id = "no_excuses",
            Title = "No Excuses",
            Description = "90% Trainingsquote über 3 Monate",
            Rarity = "EPIC",
            TargetValue = 90
        });

        // =========================
        // IMPOSSIBLE
        // =========================

        Achievements.Add(new AchievementItem
        {
            Id = "god_mode",
            Title = "God Mode Activated",
            Description = "1.000.000 kg Gesamtvolumen",
            Rarity = "IMPOSSIBLE",
            TargetValue = 1000000
        });

        Achievements.Add(new AchievementItem
        {
            Id = "titan",
            Title = "Titan",
            Description = "500 Trainingssessions",
            Rarity = "IMPOSSIBLE",
            TargetValue = 500
        });

        Achievements.Add(new AchievementItem
        {
            Id = "legend_of_iron",
            Title = "Legend of Iron",
            Description = "Ein halbes Jahr konsequent trainiert",
            Rarity = "IMPOSSIBLE",
            TargetValue = 182
        });

        Achievements.Add(new AchievementItem
        {
            Id = "unstoppable",
            Title = "Unstoppable",
            Description = "365 Tage Streak",
            Rarity = "IMPOSSIBLE",
            TargetValue = 365
        });

        Achievements.Add(new AchievementItem
        {
            Id = "human_myth",
            Title = "Human Myth",
            Description = "Mehrere EPIC Achievements gleichzeitig",
            Rarity = "IMPOSSIBLE",
            TargetValue = 3
        });

        // =========================
        // SPECIAL
        // =========================

        Achievements.Add(new AchievementItem
        {
            Id = "umweg",
            Title = "Umweg-Variante",
            Description = "Du hast eine Übungsvariante genutzt",
            Rarity = "SPECIAL",
            TargetValue = 1
        });

        Achievements.Add(new AchievementItem
        {
            Id = "gym_philosopher",
            Title = "Gym Philosopher",
            Description = "5 Notizen oder Varianten genutzt",
            Rarity = "SPECIAL",
            TargetValue = 5
        });

        Achievements.Add(new AchievementItem
        {
            Id = "late_night_beast",
            Title = "Late Night Beast",
            Description = "Training nach 22 Uhr",
            Rarity = "SPECIAL",
            TargetValue = 1
        });

        Achievements.Add(new AchievementItem
        {
            Id = "early_bird_gains",
            Title = "Early Bird Gains",
            Description = "Training vor 8 Uhr",
            Rarity = "SPECIAL",
            TargetValue = 1
        });

        Achievements.Add(new AchievementItem
        {
            Id = "comeback_king",
            Title = "Comeback King",
            Description = "Nach Pause wieder gestartet",
            Rarity = "SPECIAL",
            TargetValue = 1
        });
        Console.WriteLine("🔥 INIT COUNT: " + Achievements.Count);
    }

    public Task<List<AchievementItem>> ProcessSets(
    List<ExerciseSet> sets,
    List<Exercise> exercises)
    {
        Console.WriteLine("🚀 ACH SERVICE: ProcessSets ENTERED");
        Console.WriteLine($"📦 Sets Count: {sets?.Count ?? 0}");

        var unlocked = new List<AchievementItem>();

        // =========================
        // SAFETY
        // =========================
        if (!SystemEnabled)
            return Task.FromResult(unlocked);

        if (sets == null || sets.Count == 0)
            return Task.FromResult(unlocked);

        // =========================
        // FILTER BASED ON SYSTEM STATE
        // =========================

        var activeSets = _systemEnabledSince.HasValue
            ? sets.Where(s => s.Date >= _systemEnabledSince.Value).ToList()
            : sets;

        var lifetimeSets = activeSets; // 🔥 WICHTIG: keine alten Daten mehr

        Console.WriteLine($"📦 ACTIVE Sets Count: {activeSets.Count}");
        Console.WriteLine($"🗂 EXERCISES LOADED: {exercises?.Count ?? -1}");

        // =========================
        // LIFETIME (BASIS-STATS)
        // =========================
        UpdateTrainingCount(lifetimeSets, unlocked);
        UpdateReps(lifetimeSets, unlocked);

        // =========================
        // ACTIVE (PROGRESSION / LOGIC)
        // =========================
        UpdateVolume(activeSets, unlocked);
        UpdateStreak(activeSets, unlocked);

        UpdateSpecialAchievements(activeSets, exercises, unlocked);
        UpdateSpikeAchievements(activeSets, unlocked);

        UpdateNoExcuses(activeSets, unlocked);
        UpdateMachineMode(activeSets, unlocked);
        UpdateAktienkurs(activeSets, unlocked);
        UpdateLegendOfIron(activeSets, unlocked);
        UpdateTitan(activeSets, unlocked);

        return Task.FromResult(unlocked);
    }
    private void UpdateTrainingCount(List<ExerciseSet> sets, List<AchievementItem> unlocked)
    {
        if (sets == null || sets.Count == 0)
            return;

        var sessions = sets
            .Select(s => s.Date.Date)
            .Distinct()
            .Count();

        UpdateAsync("warmup", sessions, unlocked);
        UpdateAsync("superman", sessions, unlocked);
        UpdateAsync("set_collector", sets.Count, unlocked);
    }

    private void UpdateReps(List<ExerciseSet> sets, List<AchievementItem> unlocked)
    {
        if (sets == null || sets.Count == 0)
            return;

        var reps = sets.Sum(s => s.Reps);

        UpdateAsync("rep_builder", reps, unlocked);
    }

    private void UpdateVolume(List<ExerciseSet> sets, List<AchievementItem> unlocked)
    {
        var volume = sets
            .GroupBy(s => s.ExerciseId)
            .Select(g => g.Max(s => s.Weight * s.Reps))
            .Sum();

        UpdateAsync("hercules", volume, unlocked);
        UpdateAsync("hulk", volume, unlocked);
        UpdateAsync("god_mode", volume, unlocked);

        UpdateAsync("baeren_stark",
            sets.Any() ? sets.Max(s => s.Weight) : 0,
            unlocked);

        UpdateAsync("break_limits", 1, unlocked);
    }

    private void UpdateStreak(List<ExerciseSet> sets, List<AchievementItem> unlocked)
    {
        var days = sets.Select(s => s.Date.Date).Distinct().OrderBy(d => d).ToList();
        if (!days.Any()) return;

        int streak = 1;
        int best = 1;

        for (int i = 1; i < days.Count; i++)
        {
            if ((days[i] - days[i - 1]).Days == 1)
                streak++;
            else
                streak = 1;

            best = Math.Max(best, streak);
        }

        UpdateAsync("consistency_starter", best, unlocked);
        UpdateAsync("consistency_pro", best, unlocked);
        UpdateAsync("iron_discipline", best, unlocked);
        UpdateAsync("unstoppable", best, unlocked);
        UpdateAsync("legend_of_iron", best, unlocked);

        UpdateAsync("first_step", days.Count, unlocked);
    }

    private void UpdateSpikeAchievements(List<ExerciseSet> sets, List<AchievementItem> unlocked)
    {
        if (sets.Count < 2) return;

        var grouped = sets.GroupBy(s => s.ExerciseId);

        foreach (var group in grouped)
        {
            var ordered = group.OrderBy(s => s.Date).ToList();

            for (int i = 1; i < ordered.Count; i++)
            {
                var prev = ordered[i - 1];
                var curr = ordered[i];

                if (prev.Weight <= 0) continue;

                var increaseFactor = curr.Weight / prev.Weight;

                if (increaseFactor >= 2.0)
                {
                    UpdateAsync("are_you_natty", 1, unlocked);
                    return;
                }
            }
        }
    }

    private void UpdateNoExcuses(List<ExerciseSet> sets, List<AchievementItem> unlocked)
    {
        var last90 = DateTime.Now.AddDays(-90);

        var activeDays = sets
            .Where(s => s.Date >= last90)
            .Select(s => s.Date.Date)
            .Distinct()
            .Count();

        UpdateAsync("no_excuses", activeDays, unlocked);
    }

    private void UpdateMachineMode(List<ExerciseSet> sets, List<AchievementItem> unlocked)
    {
        var sessions = sets.Select(s => s.Date.Date).Distinct().Count();
        UpdateAsync("machine_mode", sessions, unlocked);
    }

    private void UpdateAktienkurs(List<ExerciseSet> sets, List<AchievementItem> unlocked)
    {
        var prCount = sets.GroupBy(s => s.ExerciseId).Count();
        UpdateAsync("aktienkurs", prCount, unlocked);
    }

    private void UpdateLegendOfIron(List<ExerciseSet> sets, List<AchievementItem> unlocked)
    {
        var days = sets.Select(s => s.Date.Date).Distinct().OrderBy(d => d).ToList();

        if (!days.Any())
        {
            UpdateAsync("legend_of_iron", 0, unlocked);
            return;
        }

        int streak = 1;
        int best = 1;

        for (int i = 1; i < days.Count; i++)
        {
            if ((days[i] - days[i - 1]).Days == 1)
                streak++;
            else
                streak = 1;

            best = Math.Max(best, streak);
        }

        UpdateAsync("legend_of_iron", best, unlocked);
    }

    private void UpdateTitan(List<ExerciseSet> sets, List<AchievementItem> unlocked)
    {
        var sessions = sets.Select(s => s.Date.Date).Distinct().Count();
        UpdateAsync("titan", sessions, unlocked);
    }

    private void UpdateSpecialAchievements(
    List<ExerciseSet> sets,
    List<Exercise> exercises,
    List<AchievementItem> unlocked)
    {
        if (sets == null || sets.Count == 0) return;

        bool lateNight = sets.Any(s => s.Date.Hour >= 22);
        if (lateNight)
            UpdateAsync("late_night_beast", 1, unlocked);

        bool earlyBird = sets.Any(s => s.Date.Hour < 8);
        if (earlyBird)
            UpdateAsync("early_bird_gains", 1, unlocked);

        int notes = sets.Count(s => !string.IsNullOrWhiteSpace(s.Notes));
        int variants = sets.Count(s => !string.IsNullOrWhiteSpace(s.SeatSetting));

        UpdateAsync("gym_philosopher", Math.Max(notes, variants), unlocked);

        foreach (var set in sets)
        {
            var exercise = exercises.FirstOrDefault(e => e.Id == set.ExerciseId);
            if (exercise == null) continue;

            var family = ExerciseData.GetFamily(exercise.Name);

            if (family != "Other" &&
                ExerciseData.Variants.TryGetValue(family, out var list) &&
                list.Any(v => exercise.Name.Contains(v, StringComparison.OrdinalIgnoreCase)))
            {
                var current = GetCurrent("umweg");
                UpdateAsync("umweg", current + 1, unlocked);
            }
        }
    }


    public void RegisterVariant(string exerciseName, List<AchievementItem> unlocked)
    {
        if (string.IsNullOrWhiteSpace(exerciseName))
            return;

        if (ExerciseData.IsVariant(exerciseName))
        {
            var current =
                Achievements.FirstOrDefault(x => x.Id == "umweg")?.CurrentValue ?? 0;

            UpdateAsync("umweg", current + 1, unlocked);
        }
    }
    public void RegisterNotesAndVariants(
    int notesCount,
    int variantsCount,
    List<AchievementItem> unlocked)
    {
        var score = Math.Max(notesCount, variantsCount);

        UpdateAsync("gym_philosopher", score, unlocked);
    }
    private async Task UpdateAsync(string id, double value, List<AchievementItem> unlocked)
    {
        if (!SystemEnabled)
            return;

        var a = Achievements.FirstOrDefault(x => x.Id == id);
        if (a == null) return;

        a.CurrentValue = value;

        a.Progress = a.TargetValue == 0
            ? 0
            : Math.Min(1, value / a.TargetValue);

        await _storage.SetDoubleAsync($"ach_{id}_value", value);

        bool justUnlocked = false;

        if (!a.IsUnlocked && a.Progress >= 1)
        {
            justUnlocked = true;

            Console.WriteLine("🔥 ACHIEVEMENT UNLOCK TRIGGERED: " + a.Id);

            a.IsUnlocked = true;
            a.UnlockedDate = DateTime.UtcNow;

            await _storage.SetBoolAsync($"ach_{id}_unlocked", true);
            await _storage.SetDoubleAsync($"ach_{id}_date", a.UnlockedDate.Value.Ticks);

            unlocked.Add(a);

            AchievementUnlocked?.Invoke(a);
        }

        ProgressUpdated?.Invoke();
    }

    public async Task LoadAsync()
    {
        Console.WriteLine("🔥 LOADASYNC START");
        Console.WriteLine($"🔥 COUNT BEFORE LOAD: {Achievements.Count}");
        foreach (var a in Achievements)
        {
            Console.WriteLine($"➡ {a.Id} | value={a.CurrentValue} | unlocked={a.IsUnlocked}");
            var value = await _storage.GetDoubleAsync($"ach_{a.Id}_value", 0);
            var unlocked = await _storage.GetBoolAsync($"ach_{a.Id}_unlocked", false);
            var ticks = await _storage.GetDoubleAsync($"ach_{a.Id}_date", 0);

            a.CurrentValue = value;
            a.IsUnlocked = unlocked;

            // =========================
            // DATE SAFE LOAD
            // =========================
            if (unlocked && ticks > 0)
                a.UnlockedDate = new DateTime((long)ticks);
            else
                a.UnlockedDate = null;

            // =========================
            // PROGRESS CALC
            // =========================
            a.Progress = a.TargetValue == 0
                ? 0
                : Math.Min(1, value / a.TargetValue);
        }

        // =========================
        // UI UPDATE TRIGGER
        // =========================
        ProgressUpdated?.Invoke();
    }

    // =========================
    // RESET (FIXED + VIEWMODEL COMPATIBLE)
    // =========================

    public void Reset()
    {
        foreach (var a in Achievements)
        {
            a.CurrentValue = 0;
            a.Progress = 0;
            a.IsUnlocked = false;
            a.UnlockedDate = null;

            _storage.RemoveAsync($"ach_{a.Id}_value");
            _storage.RemoveAsync($"ach_{a.Id}_unlocked");
            _storage.RemoveAsync($"ach_{a.Id}_date");
        }

        ProgressUpdated?.Invoke();
        AchievementsReset?.Invoke();
    }
    private double GetCurrent(string id)
    {
        var a = Achievements.FirstOrDefault(x => x.Id == id);
        return a?.CurrentValue ?? 0;
    }
    public List<object> GetExportData()
    {
        return Achievements.Select(a => new
        {
            a.Id,
            a.CurrentValue,
            a.IsUnlocked,
            UnlockDateTicks = a.UnlockedDate?.Ticks ?? 0
        }).ToList<object>();
    }
    public async Task Initialize()
    {
        Console.WriteLine("🔥 INITIALIZE RUN: " + GetHashCode());

        await LoadFromStorageAsync();
    }
    public async Task LoadFromStorageAsync()
    {
        // =========================
        // LOAD SYSTEM ENABLE TIME
        // =========================
        var systemSinceTicks = await _storage.GetDoubleAsync("ach_system_since", 0);

        _systemEnabledSince = systemSinceTicks > 0
            ? new DateTime((long)systemSinceTicks)
            : null;

        Console.WriteLine("🔥 SYSTEM ENABLED SINCE: " + _systemEnabledSince);

        // =========================
        // LOAD ACHIEVEMENTS
        // =========================
        foreach (var a in Achievements)
        {
            var value = await _storage.GetDoubleAsync($"ach_{a.Id}_value", 0);
            var unlocked = await _storage.GetBoolAsync($"ach_{a.Id}_unlocked", false);
            var dateTicks = await _storage.GetDoubleAsync($"ach_{a.Id}_date", 0);

            a.CurrentValue = value;

            a.IsUnlocked = unlocked || value >= a.TargetValue;

            if (a.IsUnlocked && dateTicks > 0)
                a.UnlockedDate = new DateTime((long)dateTicks);

            a.Progress = a.TargetValue == 0
                ? 0
                : Math.Min(1, value / a.TargetValue);
        }

        ProgressUpdated?.Invoke();
    }
    public async Task EnsureInitializedAsync()
    {
        if (_initialized)
            return;

        _initialized = true;

        Init(); // Struktur bauen
        await LoadFromStorageAsync(); // Daten laden

        Console.WriteLine("🔥 ACHIEVEMENTS READY");
    }
    private bool _systemEnabled = true;

    public bool SystemEnabled
    {
        get => _systemEnabled;
        set
        {
            if (_systemEnabled == value)
                return;

            _systemEnabled = value;

            if (value)
            {
                _systemEnabledSince = DateTime.UtcNow;

                // 🔥 PERSISTENT speichern
                _ = _storage.SetDoubleAsync("ach_system_since", _systemEnabledSince.Value.Ticks);
            }
            else
            {
                _systemEnabledSince = null;
            }
        }
    }
    public bool PopupsEnabled { get; set; } = true;
    public bool ShowSpecialAchievements { get; set; } = true;

    public bool IsHidden(AchievementItem a)
    {
        if (ShowSpecialAchievements)
            return false;

        return a.Rarity == "SPECIAL" || a.Rarity == "IMPOSSIBLE";
    }
}