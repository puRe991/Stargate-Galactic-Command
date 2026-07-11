using System;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class ChatService
    {
        public const int MaxBodyLength = 500;
        public const int HistoryLimit = 100;
        public static readonly TimeSpan MinInterval = TimeSpan.FromSeconds(2);

        public ServerChatMessage Send(User sender, string body, DateTime? lastMessageAtUtc, DateTime now)
        {
            if (sender == null) throw new ArgumentNullException(nameof(sender));
            if (sender.IsNpc) throw new InvalidOperationException("NSC können nicht am Chat teilnehmen.");
            body = (body ?? string.Empty).Trim();
            if (body.Length == 0) throw new ArgumentException("Nachricht darf nicht leer sein.");
            if (body.Length > MaxBodyLength) throw new ArgumentException($"Nachricht darf maximal {MaxBodyLength} Zeichen haben.");
            if (lastMessageAtUtc.HasValue && now - lastMessageAtUtc.Value < MinInterval) throw new InvalidOperationException("Bitte warte kurz, bevor du die nächste Nachricht sendest.");
            return new ServerChatMessage
            {
                ServerId = sender.ServerId,
                UserId = sender.Id,
                User = sender,
                Body = body,
                CreatedAtUtc = now
            };
        }
    }
}
