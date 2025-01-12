using Microsoft.AspNetCore.SignalR;
using SignalRWebApp.Services;

namespace SignalRWebApp.Hubs;

public class GroupHub : Hub
{
    private GroupsManager _groupsManager;
    public GroupHub(GroupsManager groupsManager)
    {
        _groupsManager = groupsManager;
    }
    public override async Task OnConnectedAsync()
    {
        ICollection<string> groups = await _groupsManager.ListGroups();
        foreach(string group in groups) {
                await Clients.Client(Context.ConnectionId).SendAsync("AddGroup", group); 
        }
    }

    public async Task EnterGroup(string groupName)
    {
        foreach (string call in await _groupsManager.addToGroup(Context.ConnectionId, groupName))
        {
            string[] id_col = call.Split(':');
            await Clients.Client(Context.ConnectionId).SendAsync("ColorTable", id_col[0], id_col[1]);
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task AddNewGroup(string groupName)
    {
        
        await _groupsManager.TryAddGroup(Context.ConnectionId, groupName);
        await Clients.All.SendAsync("AddGroup", groupName);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task ColorCell(string cellId, string color)
    {
        var groupName = await _groupsManager.ColorCell(Context.ConnectionId, cellId, color);
        await Clients.OthersInGroup(groupName).SendAsync("ColorTable", cellId, color);
    }

    public async Task QuitGroup()
    {
        string deletGroup = await _groupsManager.DropUser(Context.ConnectionId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, deletGroup);
        if (deletGroup != string.Empty)
            await Clients.All.SendAsync("DropGroup", deletGroup);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await QuitGroup();
    }
}
