using Trainingsfortschritt.Core.Models;

public interface IDatabaseService
{
    // =========================
    // EXERCISES
    // =========================
    Task<List<Exercise>> GetExercisesAsync();
    Task<Exercise?> GetExerciseByIdAsync(int id);
    Task<bool> AddExerciseAsync(Exercise ex);
    Task DeleteExerciseAsync(int id);

    // =========================
    // SETS
    // =========================
    Task<List<ExerciseSet>> GetAllSetsAsync();
    Task<List<ExerciseSet>> GetSetsForExerciseAsync(int exerciseId);
    Task AddSetAsync(ExerciseSet set);
    Task<ExerciseSet?> GetLatestSetForExerciseAsync(int exerciseId);
    Task DeleteSetAsync(int setId);

    // =========================
    // TARGETS
    // =========================
    Task SetExerciseTargetAsync(int exerciseId, double weight, int sets, int reps);

    // =========================
    // GOALS
    // =========================
    Task<List<GoalHistory>> GetGoalHistoryAsync();
    Task AddGoalHistoryAsync(GoalHistory g);
    Task ClearGoalHistoryAsync();

    // =========================
    // RESET / EXPORT
    // =========================
    Task ResetDatabaseAsync();
    Task<string> ExportDatabaseAsync();
    Task ImportDatabaseAsync();
    Task ClearAllAsync();
    Task UpdateSetAsync(ExerciseSet set);
    Task<List<ExerciseStatRecord>> GetExerciseStatRecordsAsync(int exerciseId);
    Task<List<GoalHistory>> GetGoalsAsync();
    Task ImportDatabaseFromJsonAsync(string json);

}