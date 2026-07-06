using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using StargateGalacticCommand.Data;

namespace StargateGalacticCommand.Web.Hubs
{
    // Technischer Spike fuer Phase 1 der MMO-Roadmap: beweist, dass Echtzeit-Positionssync
    // ueber SignalR innerhalb der bestehenden ASP.NET Core Session-Authentifizierung funktioniert.
    // Spielerzustand ist bewusst nur In-Memory und nicht persistiert.
    public class ExplorationHub : Hub
    {
        private const string SpikeGroup = "spike";

        private sealed class PlayerState
        {
            public int UserId { get; set; }
            public string CommanderName { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
        }

        private static readonly ConcurrentDictionary<string, PlayerState> Players = new();

        private readonly GameDbContext _db;

        public ExplorationHub(GameDbContext db)
        {
            _db = db;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var userId = httpContext?.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                Context.Abort();
                return;
            }

            var commanderName = _db.Users.Where(u => u.Id == userId.Value).Select(u => u.UserName).FirstOrDefault() ?? "Commander";

            var state = new PlayerState { UserId = userId.Value, CommanderName = commanderName, X = 300, Y = 200 };
            Players[Context.ConnectionId] = state;

            await Groups.AddToGroupAsync(Context.ConnectionId, SpikeGroup);

            var existing = Players
                .Where(kv => kv.Key != Context.ConnectionId)
                .Select(kv => new { connectionId = kv.Key, kv.Value.CommanderName, kv.Value.X, kv.Value.Y })
                .ToList();
            await Clients.Caller.SendAsync("ExistingPlayers", existing);

            await Clients.OthersInGroup(SpikeGroup).SendAsync("PlayerJoined", new { connectionId = Context.ConnectionId, commanderName = state.CommanderName, x = state.X, y = state.Y });

            await base.OnConnectedAsync();
        }

        public async Task Move(double x, double y)
        {
            if (!Players.TryGetValue(Context.ConnectionId, out var state))
            {
                return;
            }

            state.X = x;
            state.Y = y;

            await Clients.OthersInGroup(SpikeGroup).SendAsync("PlayerMoved", new { connectionId = Context.ConnectionId, x, y });
        }

        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            Players.TryRemove(Context.ConnectionId, out _);
            await Clients.OthersInGroup(SpikeGroup).SendAsync("PlayerLeft", new { connectionId = Context.ConnectionId });
            await base.OnDisconnectedAsync(exception);
        }
    }
}
