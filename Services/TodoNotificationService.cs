namespace TodoApp.Services;

public class TodoNotificationService
{
    // Fired when lists are added or deleted on the Home dashboard
    public event Func<string?, Task>? OnListsChanged;

    // Fired when items inside a specific list are added, toggled, or deleted
    public event Func<int, string?, Task>? OnListItemsChanged;

    public async Task NotifyListsChangedAsync(string? ownerId)
    {
        if (OnListsChanged is not null)
        {
            await OnListsChanged.Invoke(ownerId);
        }
    }

    public async Task NotifyListItemsChangedAsync(int listId, string? ownerId)
    {
        if (OnListItemsChanged is not null)
        {
            await OnListItemsChanged.Invoke(listId, ownerId);
        }
    }
}
