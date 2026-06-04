namespace Trainingsfortschritt.Core.Services;

public interface INotificationService
{
    Task RequestPermissionAsync();

    Task ShowNotificationAsync(string title, string message, int id = 0);

    Task ScheduleTrainingReminderAsync(bool trainedToday);

    Task ScheduleInactiveRemindersAsync(DateTime lastTrainingDate);

    void CancelInactiveReminders();

    void CancelTrainingReminder();

    Task UpdateTrainingReminderStateAsync(bool enabled, bool trainedToday);
}