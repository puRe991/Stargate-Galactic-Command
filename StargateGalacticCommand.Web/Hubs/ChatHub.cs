using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using StargateGalacticCommand.Core.Services;
using StargateGalacticCommand.Data;

namespace StargateGalacticCommand.Web.Hubs
{
    public class ChatHub : Hub
    {
        private readonly GameDbContext _db;
        private readonly ChatService _chat;

        public ChatHub(GameDbContext db, ChatService chat)
        {
            _db = db;
            _chat = chat;
        }

        public static string GroupName(int serverId) => "chat-server-" + serverId;

        public override async Task OnConnectedAsync()
        {
            int? userId = CurrentUserId();
            if (userId.HasValue)
            {
                int? serverId = _db.Users.Where(u => u.Id == userId.Value).Select(u => (int?)u.ServerId).FirstOrDefault();
                if (serverId.HasValue)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(serverId.Value));
                }
            }
            await base.OnConnectedAsync();
        }

        public async Task SendMessage(string body)
        {
            int? userId = CurrentUserId();
            if (!userId.HasValue) throw new HubException("Nicht angemeldet.");
            var sender = _db.Users.SingleOrDefault(u => u.Id == userId.Value);
            if (sender == null) throw new HubException("Nutzer nicht gefunden.");
            var now = DateTime.UtcNow;
            DateTime? lastMessageAtUtc = _db.ServerChatMessages.Where(m => m.UserId == sender.Id).OrderByDescending(m => m.CreatedAtUtc).Select(m => (DateTime?)m.CreatedAtUtc).FirstOrDefault();
            Core.Models.ServerChatMessage message;
            try
            {
                message = _chat.Send(sender, body, lastMessageAtUtc, now);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                throw new HubException(ex.Message);
            }
            _db.ServerChatMessages.Add(message);
            await _db.SaveChangesAsync();
            await Clients.Group(GroupName(sender.ServerId)).SendAsync("ReceiveMessage", new
            {
                userId = sender.Id,
                userName = sender.UserName,
                body = message.Body,
                createdAtUtc = message.CreatedAtUtc.ToString("u")
            });
        }

        private int? CurrentUserId()
        {
            var httpContext = Context.GetHttpContext();
            return httpContext?.Session.GetInt32("UserId");
        }
    }
}
