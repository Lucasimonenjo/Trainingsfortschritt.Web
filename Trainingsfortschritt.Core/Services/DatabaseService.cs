using SQLite;
using System.Data.Common;
using System.Text.Json;
using Trainingsfortschritt.Core.Abstractions;
using Trainingsfortschritt.Core.Models;

namespace Trainingsfortschritt.Core.Services;

public class DatabaseService : IDatabaseService
{
    private readonly SQLiteAsyncConnection _db;
    private readonly IStorageService _storage;
    private readonly Task _initTask;
    private readonly AchievementService _achievementService;
    public event Action? AchievementsChanged;

    private bool _initialized;

    public DatabaseService(
    IPlatformServices platform,
    IStorageService storage,
    AchievementService achievementService)
    {
        _storage = storage;
        _achievementService = achievementService;

        var path = platform.GetDatabasePath("trainingsfortschritt.db3");

        Console.WriteLine("[DB] Path: " + path);

        _db = new SQLiteAsyncConnection(path);

        _initTask = InitAsync();
    }

    private async Task InitAsync()
    {
        if (_initialized)
            return;

        await _db.CreateTableAsync<Exercise>();
        await _db.CreateTableAsync<ExerciseSet>();
        await _db.CreateTableAsync<GoalHistory>();
        await _db.CreateTableAsync<PersonalRecord>();

        _initialized = true;
    }

    private Task EnsureInit() => _initTask;

    // =========================
    // EXERCISES
    // =========================

    public async Task<List<Exercise>> GetExercisesAsync()
    {
        await EnsureInit();

        return await _db.Table<Exercise>()
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<Exercise?> GetExerciseByIdAsync(int id)
    {
        await EnsureInit();

        return await _db.Table<Exercise>()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<bool> AddExerciseAsync(Exercise ex)
    {
        await EnsureInit();

        await _db.InsertAsync(ex);

        return true;
    }

    public async Task DeleteExerciseAsync(int id)
    {
        await EnsureInit();

        var entity = await _db.Table<Exercise>()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity != null)
            await _db.DeleteAsync(entity);
    }

    // 🔥 NEU
    public async Task SetExerciseTargetAsync(
    int exerciseId,
    double targetWeight,
    int targetSets,
    int targetReps)
    {
        var exercise =
            await GetExerciseByIdAsync(exerciseId);

        if (exercise == null)
            return;

        exercise.TargetWeight = targetWeight;
        exercise.TargetSets = targetSets;
        exercise.TargetReps = targetReps;

        exercise.TargetSetDate = DateTime.UtcNow;

        await _db.UpdateAsync(exercise);
    }

    // =========================
    // SETS
    // =========================

    public async Task<List<ExerciseSet>> GetAllSetsAsync()
    {
        await EnsureInit();

        return await _db.Table<ExerciseSet>()
            .ToListAsync();
    }

    public async Task<List<ExerciseSet>> GetSetsForExerciseAsync(int exerciseId)
    {
        await EnsureInit();

        return await _db.Table<ExerciseSet>()
            .Where(s => s.ExerciseId == exerciseId)
            .OrderByDescending(s => s.Date)
            .ToListAsync();
    }

    public async Task AddSetAsync(ExerciseSet set)
    {
        await EnsureInit();

        await _db.InsertAsync(set);
    }

    // 🔥 NEU
    public async Task DeleteSetAsync(int setId)
    {
        await EnsureInit();

        var set = await _db.Table<ExerciseSet>()
            .FirstOrDefaultAsync(x => x.Id == setId);

        if (set != null)
            await _db.DeleteAsync(set);
    }

    public async Task<ExerciseSet?> GetLatestSetForExerciseAsync(int exerciseId)
    {
        await EnsureInit();

        return await _db.Table<ExerciseSet>()
            .Where(s => s.ExerciseId == exerciseId)
            .OrderByDescending(s => s.Date)
            .FirstOrDefaultAsync();
    }

    // =========================
    // GOALS
    // =========================

    public async Task AddGoalHistoryAsync(GoalHistory g)
    {
        await EnsureInit();

        await _db.InsertAsync(g);
    }

    public async Task<List<GoalHistory>> GetGoalHistoryAsync()
    {
        await EnsureInit();

        return await _db.Table<GoalHistory>()
            .OrderByDescending(g => g.DateReached)
            .ToListAsync();
    }

    // =========================
    // RESET
    // =========================

    public async Task ResetDatabaseAsync()
    {
        await EnsureInit();

        await _db.DropTableAsync<ExerciseSet>();
        await _db.DropTableAsync<Exercise>();
        await _db.DropTableAsync<GoalHistory>();
        await _db.DropTableAsync<PersonalRecord>();

        _initialized = false;

        await InitAsync();
    }

    // =========================
    // EXPORT
    // =========================

    public async Task<string> ExportAsync()
    {
        Console.WriteLine("🔥 DATABASE EXPORT");
        await EnsureInit();

        var data = new
        {
            Exercises = await _db.Table<Exercise>().ToListAsync(),
            Sets = await _db.Table<ExerciseSet>().ToListAsync(),
            Goals = await _db.Table<GoalHistory>().ToListAsync()
        };

        return JsonSerializer.Serialize(
            data,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });
    }
    public Task ClearGoalHistoryAsync()
    {
        // TODO: hier deine echte Logik einbauen
        // z.B. Ziele in der DB löschen
        return Task.CompletedTask;
    }

    public Task<string> ExportDatabaseAsync()
    {
        // TODO: echte Export-Logik
        // z.B. Pfad zur exportierten Datei zurückgeben
        var path = "export.json";
        return Task.FromResult(path);
    }

    public Task ImportDatabaseAsync()
    {
        // TODO: echte Import-Logik
        // z.B. JSON einlesen und in DB importieren
        return Task.CompletedTask;
    }
    public async Task ClearAllAsync()
    {
        await EnsureInit();

        await _db.DeleteAllAsync<ExerciseSet>();
        await _db.DeleteAllAsync<Exercise>();
        await _db.DeleteAllAsync<GoalHistory>();
        await _db.DeleteAllAsync<PersonalRecord>();
    }
    public async Task UpdateSetAsync(ExerciseSet set)
    {
        await _db.UpdateAsync(set);
    }
    public Task<List<ExerciseStatRecord>> GetExerciseStatRecordsAsync(int exerciseId)
    {
        throw new NotSupportedException(
            "PRs are handled in IndexedDbService in Web version.");
    }
    public async Task<List<GoalHistory>> GetGoalsAsync()
    {
        await EnsureInit();

        return await _db.Table<GoalHistory>()
            .OrderByDescending(g => g.DateReached)
            .ToListAsync();
    }
    public async Task ImportDatabaseFromJsonAsync(string json)
    {
        Console.WriteLine("🔥 IMPORTDATABASEFROMJSONASYNC CALLED");
        Console.WriteLine("💥 IMPORT CALLED");
        await EnsureInit();

        if (string.IsNullOrWhiteSpace(json))
        {
            Console.WriteLine("❌ JSON empty");
            return;
        }

        var data = JsonSerializer.Deserialize<ImportModel>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (data == null)
        {
            Console.WriteLine("❌ data == null");
            return;
        }

        Console.WriteLine($"📦 Exercises: {data.Exercises?.Count ?? 0}");
        Console.WriteLine($"📦 Sets: {data.Sets?.Count ?? 0}");
        Console.WriteLine($"🏆 Achievements: {data.Achievements?.Count ?? 0}");

        // =========================
        // RESET DB
        // =========================
        await _db.DeleteAllAsync<Exercise>();
        await _db.DeleteAllAsync<ExerciseSet>();
        await _db.DeleteAllAsync<GoalHistory>();

        // =========================
        // RESTORE DB
        // =========================
        if (data.Exercises?.Count > 0)
            await _db.InsertAllAsync(data.Exercises);

        if (data.Sets?.Count > 0)
            await _db.InsertAllAsync(data.Sets);

        if (data.Goals?.Count > 0)
            await _db.InsertAllAsync(data.Goals);

        // =========================
        // ACHIEVEMENTS -> STORAGE ONLY
        // =========================
        if (data.Achievements?.Count > 0)
        {
            Console.WriteLine($"🏆 Saving {data.Achievements.Count} achievements...");

            foreach (var a in data.Achievements)
            {
                await _storage.SetDoubleAsync($"ach_{a.Id}_value", a.CurrentValue);

                // 🔥 WICHTIG: sichere bool conversion
                await _storage.SetBoolAsync($"ach_{a.Id}_unlocked", a.IsUnlocked);

                await _storage.SetDoubleAsync($"ach_{a.Id}_date", a.UnlockDateTicks);
            }
        }
        await _achievementService.Initialize();

        Console.WriteLine("✅ IMPORT DONE (NO UI SYNC HERE)");
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
        public long UnlockDateTicks { get; set; }
    }
}
