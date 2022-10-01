using Microsoft.AspNetCore.SignalR;

namespace ThuInfoWeb.Hubs;

public class ScheduleSyncHub : Hub
{
    private static readonly List<SyncClient> SyncClients = new();

    public void StartMatch(string user, bool isSending)
    {
        if (SyncClients.Exists(x => x.User == user)) // Existing a user, try to match it.
        {
            if (isSending)
            {
                if (!SyncClients.Exists(x => x.User == user && x.IsSending == false))
                    return; // no receiver, just return
                var target = SyncClients.Where(x => x.User == user).First();
                SyncClients.Remove(target);
                _ = Clients.Caller.SendAsync("SetTarget", target.Id);
            }
            else // is receiving
            {
                if (!SyncClients.Exists(x => x.User == user && x.IsSending == true))
                    return; // no sender, just return
                var targetId = Context.ConnectionId;
                var sender = SyncClients.Where(x => x.User == user).First();
                SyncClients.Remove(sender);
                _ = Clients.Client(sender.Id).SendAsync("SetTarget", targetId);
            }
        }
        else // No user can be matched, waiting
        {
            SyncClients.Add(new(Context.ConnectionId, user, isSending));
        }
    }

    public void SendToTarget(string targetId, string schedulesJson)
    {
        _ = Clients.Client(targetId).SendAsync("ReceiveSchedules", schedulesJson);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (SyncClients.Exists(x => x.Id == Context.ConnectionId))
            SyncClients.Remove(SyncClients.Where(x => x.Id == Context.ConnectionId).First());
        return base.OnDisconnectedAsync(exception);
    }

    private class SyncClient
    {
        public string Id { get; }
        public string User { get; }
        public bool IsSending { get; }

        public SyncClient(string id, string user, bool isSending)
        {
            Id = id;
            User = user;
            IsSending = isSending;
        }
    }
}