using SignalRWebApp.Models;
using System.Collections;
using System.Collections.Concurrent;

namespace SignalRWebApp.Services
{
    public class GroupsManager
    {
        public readonly int tableSize = 25;

        private ConcurrentDictionary<string, Group> groupsDict = new ConcurrentDictionary<string, Group>();
        private ConcurrentDictionary<string, object> groupLockers = new ConcurrentDictionary<string, object>();
        private ConcurrentDictionary<string, string> usersDict = new ConcurrentDictionary<string, string>();

        public GroupsManager() { 
            tableSize = 25;
            /*groupsDict.TryAdd("Drawing penises",
                new Group("Drawing penises", tableSize));
            groupLockers.TryAdd("Drawing penises", new object());  - was there for test only */
        }
        public Task<string> DropUser(string connectionId)
        {
            string groupName;
            if (!usersDict.TryRemove(connectionId, out groupName))
                throw new Exception($"User {connectionId} not found");  //removing a user
            Group group;
            if (!groupsDict.TryGetValue(groupName, out group))
                throw new Exception($"Group {groupName} not found");  //getting a group the user was in
            object locker;
            if (!groupLockers.TryGetValue(groupName, out locker))   //getting a locker to safely edit this group
                throw new Exception("Something just went hell wrong here...");  //hope you'll never see this message
            string result = string.Empty;
            lock (locker)
            {
                group.participants--;
                if (group.participants == 0)
                {
                    groupsDict.Remove(groupName, out group);
                    groupLockers.Remove(groupName, out locker); //here the locker would be populated with it's own ref
                        //so I think this won't be a problem, right?
                    result = groupName;
                }
            }
            return Task<string>.FromResult(result);  //only to be able to await on this one
        }

        public Task TryAddGroup(string connectionId, string groupName)
        {
            if (!groupsDict.TryAdd(groupName, new Group(groupName, tableSize)))
                throw new InvalidOperationException("Cannot create a group (May it already exist?)");
            if (!usersDict.TryAdd(connectionId, groupName))
                throw new InvalidOperationException("Cannot create a group (May you be connected to another?)");
            if (!groupLockers.TryAdd(groupName, new object()))
                throw new InvalidOperationException("Cannot create a group (Something inner server...)");
            return Task.CompletedTask;
        }

        public Task<IEnumerable> addToGroup(string connectionId, string groupName)
        {
            if(usersDict.TryGetValue(connectionId, out var user))
            {
                throw new InvalidOperationException("User already in the group");
            }
            Group group;
            object locker;
            if (!groupsDict.TryGetValue(groupName, out group))
                throw new InvalidOperationException();
            if (!groupLockers.TryGetValue(groupName, out locker))
                throw new InvalidOperationException();
            IEnumerable result = null;
            lock (locker)
            {
                group.participants++;
                result = group.GetTableCalls();
            }
            usersDict.AddOrUpdate(connectionId, groupName, (key, value) => groupName);
            return Task<IEnumerable>.Factory.StartNew(() => result);
        }
        public Task<ICollection<string>> ListGroups()
        {
            return Task<ICollection<string>>.FromResult(groupsDict.Keys);
        }

        public Task<string> ColorCell(string connectionId, string cellId, string color)
        {
            string groupName;
            Group group;
            object locker;
            if (!usersDict.TryGetValue(connectionId, out groupName))
                throw new InvalidOperationException("Cannot access the user's group");
            if (!groupsDict.TryGetValue(groupName, out group))
                throw new InvalidOperationException("Cannot get a group");
            if (!groupLockers.TryGetValue(groupName, out locker))
                throw new InvalidOperationException("Cannot access the locker object");
            lock (locker)
            {
                group.ColorCell(cellId, color);
            }
            return Task.FromResult(groupName);
        }
    }
}
