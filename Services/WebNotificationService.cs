using Microsoft.JSInterop;
using Trainingsfortschritt.Core.Services;

namespace Trainingsfortschritt.Web.Services;

public class WebNotificationService : INotificationService
{
    private readonly IJSRuntime _js;

    public WebNotificationService(IJSRuntime js)
    {
        _js = js;
    }

    public Task RequestPermissionAsync()
        => Task.CompletedTask;

    public async Task ShowNotificationAsync(string title, string message, int id = 0)
    {
        await _js.InvokeVoidAsync(
            "appNotifications.alert",
            title,
            message
        );
    }

    public Task ScheduleTrainingReminderAsync(bool trainedToday)
        => Task.CompletedTask;

    public Task ScheduleInactiveRemindersAsync(DateTime lastTrainingDate)
        => Task.CompletedTask;

    public void CancelInactiveReminders() { }

    public void CancelTrainingReminder() { }

    public Task UpdateTrainingReminderStateAsync(bool enabled, bool trainedToday)
        => Task.CompletedTask;
}