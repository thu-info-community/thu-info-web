using System.Security.Cryptography;
using Microsoft.AspNetCore.SignalR;

namespace ThuInfoWeb.Hubs;

public class ScheduleSyncHub : Hub
{
    private static readonly List<SyncClient> SyncClients = [];
    private static readonly List<ConfirmSyncClient> ConfirmSyncClients = [];

    public void StartMatch(string user, bool isSending)
    {
        if (SyncClients.Exists(x => x.User == user)) // Existing a user, try to match it.
        {
            var token = GenToken();
            if (isSending)
            {
                if (!SyncClients.Exists(x => x.User == user && x.IsSending == false))
                    return; // no receiver, just return
                var target = SyncClients.First(x => x.User == user);
                SyncClients.Remove(target);
                _ = Clients.Clients(Context.ConnectionId, target.Id).SendAsync("ConfirmMatch", token);
            }
            else // is receiving
            {
                if (!SyncClients.Exists(x => x.User == user && x.IsSending))
                    return; // no sender, just return
                var sender = SyncClients.First(x => x.User == user);
                SyncClients.Remove(sender);
                _ = Clients.Clients(sender.Id, Context.ConnectionId).SendAsync("ConfirmMatch", token);
            }
        }
        else // No user can be matched, waiting
        {
            SyncClients.Add(new SyncClient(Context.ConnectionId, user, isSending));
        }

        return;

        static string GenToken()
        {
            var rand = Random.Shared;
            var b = new byte[16];
            rand.NextBytes(b);
            var ret = "";
            var hash = MD5.HashData(b);
            for (var i = 0; i < 3; i++)
                ret += hash[i].ToString("x").PadLeft(2, '0');

            return ret;
        }
    }

    public void ConfirmMatch(string user, string token, bool isSending)
    {
        ConfirmSyncClients.Add(new ConfirmSyncClient(Context.ConnectionId, user, isSending, token));
        var matchedClients = ConfirmSyncClients.FindAll(x => x.Token == token);
        if (matchedClients.Count != 2)
            return;
        if (!matchedClients.TrueForAll(x => x.Token == token)) // code mismatched
            return;
        _ = Clients.Client(matchedClients.Find(x => x.IsSending)!.Id)
            .SendAsync("SetTarget", matchedClients.Find(x => !x.IsSending)!.Id);
        if (ConfirmSyncClients.Count > 100)
            ConfirmSyncClients.Clear();
    }

    public void SendToTarget(string targetId, string schedulesJson)
    {
        _ = Clients.Client(targetId).SendAsync("ReceiveSchedules", schedulesJson);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (SyncClients.Exists(x => x.Id == Context.ConnectionId))
            SyncClients.Remove(SyncClients.First(x => x.Id == Context.ConnectionId));
        if (ConfirmSyncClients.Exists(x => x.Id == Context.ConnectionId))
            ConfirmSyncClients.Remove(ConfirmSyncClients.First(x => x.Id == Context.ConnectionId));
        return base.OnDisconnectedAsync(exception);
    }

    private class SyncClient(string id, string user, bool isSending)
    {
        public string Id { get; } = id;
        public string User { get; } = user;
        public bool IsSending { get; } = isSending;
    }

    private class ConfirmSyncClient(string id, string user, bool isSending, string token)
        : SyncClient(id, user, isSending)
    {
        public string Token { get; } = token;
    }
}
