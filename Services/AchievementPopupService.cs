using Trainingsfortschritt.Core.Models;

namespace Trainingsfortschritt.Web.Services;

public class AchievementPopupService
{
    private readonly Queue<AchievementItem> _queue = new();

    public event Action? Changed;

    public List<AchievementItem> CurrentBatch { get; private set; } = new();

    private bool _isShowing;
    private CancellationTokenSource? _cts;

    // =========================
    // ADD ACHIEVEMENT
    // =========================
    public void Queue(AchievementItem achievement)
    {
        if (achievement == null)
            return;

        _queue.Enqueue(achievement);
    }

    // =========================
    // CALL THIS ONCE AFTER PROCESSSETS
    // =========================
    public async Task ShowAsync()
    {
        if (_isShowing)
            return;

        if (_queue.Count == 0)
            return;

        _isShowing = true;

        // =========================
        // ALL IN ONE BATCH
        // =========================
        CurrentBatch = _queue.ToList();
        _queue.Clear();

        Changed?.Invoke();

        // =========================
        // SHOW 5 SECONDS
        // =========================
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        try
        {
            await Task.Delay(3000, _cts.Token);
        }
        catch
        {
            // cancelled
        }

        // =========================
        // CLOSE
        // =========================
        CurrentBatch.Clear();
        Changed?.Invoke();

        _isShowing = false;
    }
}