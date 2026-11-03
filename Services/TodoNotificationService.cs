namespace TodoApp.Services;

public class TodoNotificationService
{
    // Fired when lists are added or deleted on the Home dashboard
    public event Func<Task>? OnListsChanged;

    // Fired when items inside a specific list are added, toggled, or deleted
    public event Func<int, Task>? OnListItemsChanged;

    public async Task NotifyListsChangedAsync()
    {
        if (OnListsChanged is not null)
        {
            await OnListsChanged.Invoke();
        }
    }

    public async Task NotifyListItemsChangedAsync(int listId)
    {
        if (OnListItemsChanged is not null)
        {
            await OnListItemsChanged.Invoke(listId);
        }
    }
}
