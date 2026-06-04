using System.Collections.ObjectModel;
using System.Linq;
using Trainingsfortschritt.Core.Models;
using Trainingsfortschritt.Core.Services;

namespace Trainingsfortschritt.Web.ViewModels;

public class AchievementsViewModel
{
    private readonly AchievementService _service;
    private readonly INotificationService _notification;
    private readonly Func<string, Task<bool>> _confirm;

    public AchievementsViewModel(
    AchievementService service,
    INotificationService notification,
    Func<string, Task<bool>> confirm)
    {
        _service = service;
        _notification = notification;
        _confirm = confirm;

        Console.WriteLine("VIEWMODEL SERVICE INSTANCE: " + _service.GetHashCode());

        _service.AchievementUnlocked -= OnAchievementUnlocked;
        _service.AchievementUnlocked += OnAchievementUnlocked;

        _service.ProgressUpdated -= Refresh;
        _service.ProgressUpdated += Refresh;

        Console.WriteLine("SUBSCRIBED TO ACHIEVEMENT EVENT");;

        Load();
    }

    // =========================
    // DATA
    // =========================

    public ObservableCollection<AchievementItem> Achievements { get; } = new();
    public ObservableCollection<AchievementItem> FilteredAchievements { get; } = new();

    private string _filter = "ALL";

    // =========================
    // STATUS
    // =========================

    public bool SystemEnabled
    {
        get => _service.SystemEnabled;
        set => _service.SystemEnabled = value;
    }

    public bool ShowSpecialAchievements
    {
        get => _service.ShowSpecialAchievements;
        set
        {
            _service.ShowSpecialAchievements = value;
            ApplyFilter();
        }
    }

    public bool PopupsEnabled
    {
        get => _service.PopupsEnabled;
        set => _service.PopupsEnabled = value;
    }

    public bool HighlightRareAchievements { get; set; } = true;

    public string SystemStatusText =>
        SystemEnabled ? "AKTIV" : "DEAKTIVIERT";

    public string SystemStatusColor =>
        SystemEnabled ? "green" : "red";

    // =========================
    // LOAD
    // =========================

    public void Load()
    {
        Achievements.Clear();

        foreach (var a in _service.Achievements)
            Achievements.Add(a);

        ApplyFilter();
    }

    private void Refresh()
    {
        Load();
        ApplyFilter();
        SettingsChanged?.Invoke();
    }

    public static event Action? SettingsChanged;

    // =========================
    // FILTER
    // =========================

    public void SetFilter(string filter)
    {
        _filter = filter;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var list = Achievements.AsEnumerable();

        if (!ShowSpecialAchievements)
        {
            list = list.Where(a =>
                !string.Equals(a.Rarity, "SPECIAL", System.StringComparison.OrdinalIgnoreCase));
        }

        if (_filter != "ALL")
        {
            list = list.Where(a =>
                string.Equals(a.Rarity, _filter, System.StringComparison.OrdinalIgnoreCase));
        }

        FilteredAchievements.Clear();

        foreach (var item in list)
            FilteredAchievements.Add(item);

        SettingsChanged?.Invoke();
    }

    // =========================
    // POPUP
    // =========================

    private void OnAchievementUnlocked(AchievementItem achievement)
    {
        Console.WriteLine("🎯 VIEWMODEL EVENT RECEIVED: " + achievement.Title);

        if (!_service.PopupsEnabled)
        {
            Console.WriteLine("❌ POPUPS DISABLED");
            return;
        }

        _ = _notification.ShowNotificationAsync(
            "🏆 Achievement freigeschaltet",
            achievement.Title
        );
    }

    // =========================
    // RESET
    // =========================

    public async Task ResetAsync()
    {
        var confirm = await _confirm(
            "Willst du wirklich alle Achievements zurücksetzen?"
        );

        if (!confirm)
            return;

        _service.Reset();

        await _notification.ShowNotificationAsync(
            "Erledigt",
            "Achievements wurden zurückgesetzt"
        );

        Load();
        SettingsChanged?.Invoke();
    }

    // =========================
    // HELPERS
    // =========================

    public bool IsRare(AchievementItem item)
    {
        return item?.Rarity is "EPIC" or "LEGENDARY" or "IMPOSSIBLE" or "SPECIAL";
    }

    public string GetColor(AchievementItem item)
    {
        if (!HighlightRareAchievements)
            return "white";

        return item?.Rarity?.ToUpperInvariant() switch
        {
            "EPIC" => "purple",
            "LEGENDARY" => "gold",
            "IMPOSSIBLE" => "red",
            "SPECIAL" => "cyan",
            _ => "white"
        };
    }
    private async Task ShowPopupSafeAsync(AchievementItem achievement)
    {
        try
        {
            await _notification.ShowNotificationAsync(
                "🏆 Achievement freigeschaltet",
                achievement.Title
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine("POPUP ERROR: " + ex.Message);
        }
    }
}