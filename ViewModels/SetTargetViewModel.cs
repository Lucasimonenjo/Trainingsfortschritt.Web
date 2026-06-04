using Trainingsfortschritt.Core.Abstractions;

namespace Trainingsfortschritt.Web.ViewModels;

public class SetTargetViewModel
{
    private readonly IDatabaseService _databaseService;

    private int _exerciseId;

    private string _targetWeightString = string.Empty;
    private string _targetRepsInput = string.Empty;

    private bool _isLoaded;
    private bool _isBusy;

    // =========================
    // CONSTRUCTOR
    // =========================

    public SetTargetViewModel(
        IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    // =========================
    // STATE
    // =========================

    public bool IsLoaded
    {
        get => _isLoaded;
        private set => _isLoaded = value;
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => _isBusy = value;
    }

    // =========================
    // PROPERTIES
    // =========================

    public int ExerciseId
    {
        get => _exerciseId;
        set => _exerciseId = value;
    }

    public string TargetWeightString
    {
        get => _targetWeightString;
        set => _targetWeightString = value;
    }

    public string TargetRepsInput
    {
        get => _targetRepsInput;
        set => _targetRepsInput = value;
    }

    // =========================
    // LOAD
    // =========================

    public async Task LoadAsync()
    {
        if (ExerciseId <= 0)
            return;

        IsLoaded = false;

        var exercise =
            await _databaseService
                .GetExerciseByIdAsync(ExerciseId);

        if (exercise != null)
        {
            // =========================
            // WEIGHT
            // =========================

            if (exercise.TargetWeight > 0)
            {
                TargetWeightString =
                    exercise.TargetWeight.ToString();
            }

            // =========================
            // SETS x REPS
            // =========================

            if (exercise.TargetSets > 0 &&
                exercise.TargetReps > 0)
            {
                TargetRepsInput =
                    $"{exercise.TargetSets}x{exercise.TargetReps}";
            }
        }

        IsLoaded = true;
    }

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
            // =========================
            // WEIGHT
            // =========================

            double weight = 0;

            if (double.TryParse(
                TargetWeightString,
                out double parsedWeight))
            {
                weight = parsedWeight;
            }

            // =========================
            // SETS + REPS
            // =========================

            int sets = 1;
            int reps = 1;

            if (!string.IsNullOrWhiteSpace(
                TargetRepsInput))
            {
                var input =
                    TargetRepsInput
                        .Trim()
                        .ToLower()
                        .Replace(" ", "");

                // 3x8
                if (input.Contains("x"))
                {
                    var parts = input.Split('x');

                    if (parts.Length == 2)
                    {
                        if (!int.TryParse(parts[0], out sets))
                            sets = 1;

                        if (!int.TryParse(parts[1], out reps))
                            reps = 1;
                    }
                }
                else
                {
                    // Nur Wiederholungen
                    if (!int.TryParse(input, out reps))
                        reps = 1;

                    sets = 1;
                }
            }

            // =========================
            // SAVE
            // =========================

            await _databaseService.SetExerciseTargetAsync(
                ExerciseId,
                weight,
                sets,
                reps);
        }
        finally
        {
            IsBusy = false;
        }
    }
}