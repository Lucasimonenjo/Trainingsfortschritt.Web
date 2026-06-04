using Microsoft.JSInterop;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;
using Trainingsfortschritt.Core.Abstractions;
using Trainingsfortschritt.Core.Services;
using Trainingsfortschritt.Web.Services;

namespace Trainingsfortschritt.Web.ViewModels;

public class SettingsViewModel
{
    private readonly IDatabaseService _databaseService;
    private readonly INotificationService _notificationService;
    private readonly AchievementService _achievementService;
    private readonly IJSRuntime _js;
    private readonly IndexedDbService _indexedDb;
    private readonly Func<string, Task<bool>> _confirm;
    private const string ACH_SYSTEM_KEY = "ach_system_enabled";
    private const string ACH_POPUP_KEY = "ach_popups_enabled";
    private const string ACH_SPECIAL_KEY = "ach_special_enabled";

    public SettingsViewModel(
        IDatabaseService databaseService,
        INotificationService notificationService,
        AchievementService achievementService,
        IJSRuntime js,
        IndexedDbService indexedDb,
        Func<string, Task<bool>> confirm)
    {
        _databaseService = databaseService;
        _notificationService = notificationService;
        _achievementService = achievementService;
        _js = js;
        _indexedDb = indexedDb;
        _confirm = confirm;

        // COMMANDS
        OpenChartCommand = new SimpleCommand(OpenChart);
        OpenPersonalRecordsCommand = new SimpleCommand(OpenPersonalRecords);
        OpenStatisticsDashboardCommand = new SimpleCommand(OpenStatisticsDashboard);

        ClearGoalHistoryCommand = new AsyncCommand(ClearGoalHistoryAsync);
        ResetAchievementsCommand = new AsyncCommand(ResetAchievementsAsync);
        ExportDataCommand = new AsyncCommand(ExportDataAsync);
        ImportDataCommand = new AsyncCommand(ImportDataAsync);
        ResetDatabaseCommand = new AsyncCommand(ResetDatabaseAsync);

        OpenExportFileCommand = new SimpleCommand(OpenExportFile);
        ShareExportFileCommand = new SimpleCommand(ShareExportFile);
        ShowDatabasePathCommand = new SimpleCommand(ShowDatabasePath);
        TestNotificationCommand = new AsyncCommand(TestNotificationAsync);
    }

    // =========================
    // THEME (PERSISTENT)
    // =========================

    private const string THEME_KEY = "app_theme";
    private const string COMPACT_KEY = "compact_cards";
    private const string GROUP_KEY = "group_exercises";

    public static event Action? ThemeChanged;
    public static event Action? SettingsChanged;

    private string _appTheme = "System";

    public string AppTheme
    {
        get => _appTheme;
        set
        {
            if (_appTheme == value)
                return;

            _appTheme = value;

            _ = SaveThemeAsync();
            ThemeChanged?.Invoke();
        }
    }

    public async Task LoadAsync()
    {
        try
        {
            var theme = await _js.InvokeAsync<string?>("idbStorage.get", THEME_KEY);
            _appTheme = string.IsNullOrWhiteSpace(theme) ? "System" : theme;

            _compactCards = await ReadBool(COMPACT_KEY);
            _groupExercises = await ReadBool(GROUP_KEY);
            _achievementService.SystemEnabled = await ReadBool(ACH_SYSTEM_KEY);
            _achievementService.PopupsEnabled = await ReadBool(ACH_POPUP_KEY);
            _achievementService.ShowSpecialAchievements = await ReadBool(ACH_SPECIAL_KEY);

            ThemeChanged?.Invoke();
            SettingsChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine("SETTINGS LOAD FAILED: " + ex.Message);

            _appTheme = "System";
            _compactCards = false;
            _groupExercises = true;
        }
    }

    private async Task SaveThemeAsync()
    {
        await _js.InvokeVoidAsync("idbStorage.set", THEME_KEY, _appTheme);
    }

    // =========================
    // SETTINGS
    // =========================

    private bool _compactCards;
    private bool _groupExercises = true;

    public bool CompactCards
    {
        get => _compactCards;
        set
        {
            if (_compactCards == value)
                return;

            _compactCards = value;
            _ = SaveBool(COMPACT_KEY, value);
            SettingsChanged?.Invoke();
        }
    }

    public bool GroupExercises
    {
        get => _groupExercises;
        set
        {
            if (_groupExercises == value)
                return;

            _groupExercises = value;
            _ = SaveBool(GROUP_KEY, value);
            SettingsChanged?.Invoke();
        }
    }
    public bool ShowProgressBars { get; set; } = true;

    public bool GoalDetectionEnabled { get; set; } = true;
    public bool VariantPopupEnabled { get; set; } = true;
    public bool TagsEnabled { get; set; } = true;
    public bool PRDetectionEnabled { get; set; } = true;

    public bool SaveSeatSettings { get; set; } = true;
    public bool SaveLastWeight { get; set; } = true;
    public bool SaveLastReps { get; set; } = true;

    public bool TrainingRemindersEnabled { get; set; } = true;
    public bool PRNotificationsEnabled { get; set; } = true;
    public bool GoalNotificationsEnabled { get; set; } = true;
    public bool InactivityNotificationsEnabled { get; set; } = true;

    public bool AchievementSystemEnabled
    {
        get => _achievementService.SystemEnabled;
        set
        {
            _achievementService.SystemEnabled = value;
            _ = SaveBool(ACH_SYSTEM_KEY, value);
            SettingsChanged?.Invoke();
        }
    }

    public bool AchievementPopupsEnabled
    {
        get => _achievementService.PopupsEnabled;
        set
        {
            _achievementService.PopupsEnabled = value;
            _ = SaveBool(ACH_POPUP_KEY, value);
            SettingsChanged?.Invoke();
        }
    }

    public bool ShowSpecialAchievements
    {
        get => _achievementService.ShowSpecialAchievements;
        set
        {
            _achievementService.ShowSpecialAchievements = value;
            _ = SaveBool(ACH_SPECIAL_KEY, value);

            SettingsChanged?.Invoke();
        }
    }

    // =========================
    // FILE STATE
    // =========================

    public string? ExportFilePath { get; set; }
    public string? ImportFilePath { get; set; }
    public string ImportContent { get; set; } = string.Empty;

    public bool HasExportFile => !string.IsNullOrWhiteSpace(ExportFilePath);
    public bool HasImportFile => !string.IsNullOrWhiteSpace(ImportFilePath);

    // =========================
    // COMMANDS
    // =========================

    public ICommand OpenChartCommand { get; }
    public ICommand OpenPersonalRecordsCommand { get; }
    public ICommand OpenStatisticsDashboardCommand { get; }

    public ICommand ClearGoalHistoryCommand { get; }
    public ICommand ResetAchievementsCommand { get; }
    public ICommand ExportDataCommand { get; }
    public ICommand ImportDataCommand { get; }
    public ICommand ResetDatabaseCommand { get; }

    public ICommand OpenExportFileCommand { get; }
    public ICommand ShareExportFileCommand { get; }
    public ICommand ShowDatabasePathCommand { get; }
    public ICommand TestNotificationCommand { get; }

    // =========================
    // ACTIONS
    // =========================

    public void OpenChart() => SettingsChanged?.Invoke();
    public void OpenPersonalRecords() => SettingsChanged?.Invoke();
    public void OpenStatisticsDashboard() => SettingsChanged?.Invoke();

    public async Task ClearGoalHistoryAsync()
        => await _databaseService.ClearGoalHistoryAsync();

    public Task ResetAchievementsAsync()
    {
        _achievementService.Reset();
        return Task.CompletedTask;
    }

    public async Task ExportDataAsync()
    {
        ExportContent =
            await _databaseService.ExportDatabaseAsync();

        ExportFilePath =
            $"training_export_{DateTime.Now:yyyy-MM-dd_HH-mm}.json";
    }

    public async Task ImportDataAsync()
    {
        if (string.IsNullOrWhiteSpace(ImportContent))
        {
            Console.WriteLine("⚠️ Import aborted: empty content");
            return;
        }

        try
        {
            Console.WriteLine("🚀 IMPORT START (UI)");

            await _databaseService.ImportDatabaseFromJsonAsync(ImportContent);

            Console.WriteLine("✅ Database import finished");

            ImportContent = string.Empty;
            ImportFilePath = null;

            SettingsChanged?.Invoke();

            // =========================
            // 🔥 ACHIEVEMENT SYNC FIX (IMPORTANT)
            // =========================
            if (_achievementService != null)
            {
                Console.WriteLine("🔄 Reinitializing Achievements...");

                // 🔥 WICHTIG: FULL RESET + LOAD
                await _achievementService.Initialize();

                Console.WriteLine("✅ Achievements fully reloaded");
            }
            else
            {
                Console.WriteLine("⚠️ AchievementService is NULL");
            }

            // =========================
            // NOTIFICATION
            // =========================
            if (_notificationService != null)
            {
                await _notificationService.ShowNotificationAsync(
                    "✅ Import erfolgreich",
                    "Alle Daten inkl. Achievements wurden geladen"
                );
            }

            Console.WriteLine("🏁 IMPORT COMPLETE");
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ IMPORT ERROR:");
            Console.WriteLine(ex);

            if (_notificationService != null)
            {
                await _notificationService.ShowNotificationAsync(
                    "❌ Import Fehler",
                    ex.Message
                );
            }
        }
    }

    public async Task ResetDatabaseAsync()
        => await _databaseService.ResetDatabaseAsync();

    public async void OpenExportFile()
    {
        if (string.IsNullOrWhiteSpace(ExportFilePath))
            return;

        if (string.IsNullOrWhiteSpace(ExportContent))
            return;

        await _js.InvokeVoidAsync(
            "downloadFile",
            ExportFilePath,
            ExportContent
        );
    }

    public async void ShareExportFile()
    {
        if (string.IsNullOrWhiteSpace(ExportFilePath))
            return;

        if (string.IsNullOrWhiteSpace(ExportContent))
            return;

        await _js.InvokeVoidAsync(
            "shareFile",
            ExportFilePath,
            ExportContent
        );
    }

    public void ShowDatabasePath()
    {
        Console.WriteLine(DatabasePath);
        ShowDatabasePathPopup = true;
        SettingsChanged?.Invoke();
    }

    public async Task TestNotificationAsync()
    {
        await _notificationService.ShowNotificationAsync(
            "🧪 Test",
            "Benachrichtigung funktioniert!");
    }

    // =========================
    // PATH
    // =========================

    public string DatabasePath =>
        Path.Combine("app", "trainingsfortschritt.db3");
    private async Task SaveCompactAsync()
    {
        await _js.InvokeVoidAsync("idbStorage.set", COMPACT_KEY, _compactCards.ToString());
    }
    private async Task<bool> ReadBool(string key)
    {
        var val = await _js.InvokeAsync<string?>("idbStorage.get", key);

        if (string.IsNullOrWhiteSpace(val))
            return false;

        return val == "1" || val.ToLower() == "true";
    }
    private async Task SaveBool(string key, bool value)
    {
        await _js.InvokeVoidAsync("idbStorage.set", key, value ? "1" : "0");
    }
    public string? LastExportFile { get; set; }
    public string ExportContent { get; set; } = string.Empty;

    public async Task PickImportFileAsync()
    {
        var result = await _js.InvokeAsync<ImportFileResult>("pickFile");

        if (result == null || string.IsNullOrWhiteSpace(result.content))
            return;

        ImportFilePath = result.name;
        ImportContent = result.content;

        SettingsChanged?.Invoke();
    }
    public async Task ResetDb()
    {
        try
        {
            // 🔥 Sicherheitsabfrage
            var confirm = await _confirm(
                "⚠️ Datenbank wirklich zurücksetzen?\n\nAlle Daten werden unwiderruflich gelöscht!"
            );

            if (!confirm)
                return;

            // 🔥 echte DB zurücksetzen
            await _databaseService.ResetDatabaseAsync();

            // =========================
            // UI STATE RESET
            // =========================

            ImportContent = string.Empty;
            ImportFilePath = null;
            ExportContent = string.Empty;
            ExportFilePath = null;

            // optional sauberer Trigger für Binding
            SettingsChanged?.Invoke();

            // =========================
            // SUCCESS POPUP
            // =========================

            if (_notificationService != null)
            {
                await _notificationService.ShowNotificationAsync(
                    "🗑 Datenbank zurückgesetzt",
                    "Alle Daten wurden erfolgreich gelöscht."
                );
            }
        }
        catch (Exception ex)
        {
            // =========================
            // ERROR POPUP
            // =========================

            if (_notificationService != null)
            {
                await _notificationService.ShowNotificationAsync(
                    "❌ Fehler beim Zurücksetzen",
                    ex.Message
                );
            }
            else
            {
                Console.WriteLine(ex);
            }
        }
    }
    public bool ShowDatabasePathPopup { get; set; }

    public void CloseDatabasePath()
    {
        ShowDatabasePathPopup = false;
        SettingsChanged?.Invoke();
    }
}