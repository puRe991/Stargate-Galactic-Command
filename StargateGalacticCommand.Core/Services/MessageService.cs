using System;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class MessageService
    {
        public const int MaxSubjectLength = 120;
        public const int MaxBodyLength = 4000;

        public PlayerMessage Send(User sender, User recipient, string subject, string body, DateTime now)
        {
            if (sender == null || recipient == null) throw new ArgumentNullException();
            if (sender.Id == recipient.Id) throw new InvalidOperationException("Du kannst dir selbst keine Nachricht senden.");
            if (recipient.IsNpc) throw new InvalidOperationException("An NSC können keine Nachrichten gesendet werden.");
            subject = (subject ?? string.Empty).Trim();
            body = (body ?? string.Empty).Trim();
            if (subject.Length == 0) throw new ArgumentException("Betreff darf nicht leer sein.");
            if (subject.Length > MaxSubjectLength) throw new ArgumentException($"Betreff darf maximal {MaxSubjectLength} Zeichen haben.");
            if (body.Length == 0) throw new ArgumentException("Nachricht darf nicht leer sein.");
            if (body.Length > MaxBodyLength) throw new ArgumentException($"Nachricht darf maximal {MaxBodyLength} Zeichen haben.");
            return new PlayerMessage
            {
                SenderUserId = sender.Id,
                SenderUser = sender,
                RecipientUserId = recipient.Id,
                RecipientUser = recipient,
                Subject = subject,
                Body = body,
                CreatedAtUtc = now
            };
        }

        public void MarkRead(PlayerMessage message, int readerUserId, DateTime now)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (message.RecipientUserId != readerUserId) throw new InvalidOperationException("Nur der Empfänger kann die Nachricht als gelesen markieren.");
            if (!message.ReadAtUtc.HasValue) message.ReadAtUtc = now;
        }
    }
}
