using System.Security.Cryptography;
using Microsoft.AspNetCore.SignalR;

namespace ThuInfoWeb.Hubs;

public sealed class ScheduleSyncHub : Hub
{
    private static readonly Lock ClientsLock = new();
    private static readonly List<SyncClient> SyncClients = [];
    private static readonly List<ConfirmSyncClient> ConfirmSyncClients = [];

    public async Task StartMatch(string user, bool isSending)
    {
        SyncClient? matchedClient;
        lock (ClientsLock)
        {
            SyncClients.RemoveAll(x => x.Id == Context.ConnectionId);
            matchedClient = SyncClients.FirstOrDefault(x => x.User == user && x.IsSending != isSending);
            if (matchedClient is null)
                SyncClients.Add(new SyncClient(Context.ConnectionId, user, isSending));
            else
                SyncClients.Remove(matchedClient);
        }

        if (matchedClient is null)
            return;

        await Clients.Clients(Context.ConnectionId, matchedClient.Id)
            .SendAsync("ConfirmMatch", GenerateToken());
    }

    public async Task ConfirmMatch(string user, string token, bool isSending)
    {
        ConfirmSyncClient? sender;
        ConfirmSyncClient? receiver;
        lock (ClientsLock)
        {
            ConfirmSyncClients.RemoveAll(x => x.Id == Context.ConnectionId);
            ConfirmSyncClients.Add(new ConfirmSyncClient(Context.ConnectionId, user, isSending, token));

            var matchedClients = ConfirmSyncClients
                .Where(x => x.User == user && x.Token == token)
                .ToList();
            if (matchedClients.Count != 2)
                return;

            sender = matchedClients.SingleOrDefault(x => x.IsSending);
            receiver = matchedClients.SingleOrDefault(x => !x.IsSending);
            if (sender is null || receiver is null)
                return;

            ConfirmSyncClients.RemoveAll(x => x.Token == token && x.User == user);
        }

        await Clients.Client(sender.Id).SendAsync("SetTarget", receiver.Id);
    }

    public Task SendToTarget(string targetId, string schedulesJson)
    {
        return Clients.Client(targetId).SendAsync("ReceiveSchedules", schedulesJson);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        lock (ClientsLock)
        {
            SyncClients.RemoveAll(x => x.Id == Context.ConnectionId);
            ConfirmSyncClients.RemoveAll(x => x.Id == Context.ConnectionId);
        }

        return base.OnDisconnectedAsync(exception);
    }

    private static string GenerateToken()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(3)).ToLowerInvariant();
    }

    private class SyncClient(string id, string user, bool isSending)
    {
        public string Id { get; } = id;
        public string User { get; } = user;
        public bool IsSending { get; } = isSending;
    }

    private sealed class ConfirmSyncClient(string id, string user, bool isSending, string token)
        : SyncClient(id, user, isSending)
    {
        public string Token { get; } = token;
    }
}
