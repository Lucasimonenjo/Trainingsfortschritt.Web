using System.Globalization;
using Trainingsfortschritt.Core.Abstractions;
using Trainingsfortschritt.Core.Helpers;
using Trainingsfortschritt.Core.Models;
using Microsoft.JSInterop;

namespace Trainingsfortschritt.Web.ViewModels;

public class AddExerciseViewModel
{
    private readonly IDatabaseService _databaseService;
    private readonly Func<string, Task<bool>> _confirm;
    private readonly IJSRuntime _js;

    public AddExerciseViewModel(
        IDatabaseService databaseService,
        Func<string, Task<bool>> confirm,
        IJSRuntime js)
    {
        _databaseService = databaseService;
        _confirm = confirm;
        _js = js;
    }

    public string Name { get; set; } = string.Empty;

    public async Task<bool> SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return false;

        string input = Name.Trim();

        string family = NormalizeFamily(input);
        var familyVariants = GetFamilyVariants(family);

        // =========================
        // SETTINGS CHECK
        // =========================
        var enabled = await _js.InvokeAsync<string?>(
            "localStorage.getItem",
            "variantPopupEnabled"
        );

        bool variantPopupEnabled = enabled == "true";

        bool addVariants = false;

        // =========================
        // POPUP LOGIK
        // =========================
        if (variantPopupEnabled && family != "Other" && familyVariants.Count > 1)
        {
            addVariants = await _confirm(
                $"„{input}“ wurde als „{family}“ erkannt.\n\nVarianten ebenfalls hinzufügen?"
            );
        }

        // =========================
        // ALLE EXISTIERENDEN EXERCISES LADEN
        // =========================
        var existing = await _databaseService.GetExercisesAsync();

        bool Exists(string name)
            => existing.Any(x =>
                x.Name.Trim().Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));

        // =========================
        // MAIN EXERCISE (nur wenn NICHT vorhanden)
        // =========================
        if (!Exists(input))
        {
            await _databaseService.AddExerciseAsync(new Exercise
            {
                Name = input
            });
        }

        // =========================
        // VARIANTS (nur neue!)
        // =========================
        if (addVariants)
        {
            foreach (var v in familyVariants)
            {
                if (string.Equals(v, input, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (Exists(v))
                    continue;

                await _databaseService.AddExerciseAsync(new Exercise
                {
                    Name = v
                });
            }
        }

        return true;
    }

    // =========================
    // FAMILY DETECTION
    // =========================
    private string NormalizeFamily(string name)
    {
        name = name.ToLowerInvariant();

        if (name.Contains("lat") || name.Contains("pull"))
            return "Pulldown";

        if (name.Contains("bench"))
            return "Bench Press";

        if (name.Contains("squat"))
            return "Squats";

        if (name.Contains("deadlift"))
            return "Deadlift";

        if (name.Contains("row"))
            return "Row";

        return "Other";
    }

    private List<string> GetFamilyVariants(string family)
    {
        if (string.IsNullOrWhiteSpace(family) || family == "Other")
            return new();

        var variants = ExerciseData.Families
            .Where(x => x.Value.Equals(family, StringComparison.OrdinalIgnoreCase))
            .Select(x => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(x.Key))
            .Distinct()
            .ToList();

        var mainName = family;

        if (!variants.Any(v => v.Equals(mainName, StringComparison.OrdinalIgnoreCase)))
            variants.Insert(0, mainName);

        return variants;
    }
}