namespace ISISLab.HelpDesk.Services
{
  using System.Collections.Concurrent;
  using System.Collections.Generic;

  using Discord.WebSocket;

  public class InteractionLockService
  {
    private readonly List<string> _targetCommands;
    private readonly ConcurrentDictionary<ulong, bool> _lockedUsers;

    public InteractionLockService()
    {
      _targetCommands = new List<string>();
      _lockedUsers    = new ConcurrentDictionary<ulong, bool>();
    }

    public InteractionLockService Register(string command)
    {
      if (_targetCommands.Contains(command))
        return this;

      _targetCommands.Add(command);
      return this;
    }

    public InteractionLockService Unregister(string command)
    {
      _targetCommands.Remove(command);
      return this;
    }

    public bool AddUserToInteractionLock(SocketUser user, string command)
    {
      if (!_targetCommands.Contains(command)) return false;
      if (_lockedUsers.ContainsKey(user.Id)) return true;

      return _lockedUsers.TryAdd(user.Id, true);
    }

    public bool RemoveUserFromInteractionLock(SocketUser user)
    {
      return _lockedUsers.TryRemove(user.Id, out var _);
    }

    public bool IsUserInInteractionLock(SocketUser user)
    {
      return _lockedUsers.ContainsKey(user.Id);
    }
  }
}
