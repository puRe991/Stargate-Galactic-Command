using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class LoginLockoutStatus
    {
        public bool IsLockedOut { get; set; }
        public DateTime? RetryAfterUtc { get; set; }
    }

    public class LoginSecurityService
    {
        public const int RetentionDays = 30;

        private sealed class LockoutTier
        {
            public int Attempts { get; }
            public int WindowMinutes { get; }
            public int LockoutMinutes { get; }
            public LockoutTier(int attempts, int windowMinutes, int lockoutMinutes)
            {
                Attempts = attempts; WindowMinutes = windowMinutes; LockoutMinutes = lockoutMinutes;
            }
        }

        // Gestaffelte Schwellen: kurze Fehlversuchsserien führen zu kurzen Sperren,
        // anhaltende Angriffe eskalieren auf längere Sperrzeiten.
        private static readonly LockoutTier[] AccountTiers =
        {
            new LockoutTier(5, 15, 15),
            new LockoutTier(10, 60, 60),
            new LockoutTier(20, 24 * 60, 24 * 60),
        };

        // IP-Schwellen liegen höher als Account-Schwellen, damit ein einzelner
        // Angreifer mit vielen Zielaccounts nicht sofort ganze Serverbereiche sperrt.
        private static readonly LockoutTier[] IpTiers =
        {
            new LockoutTier(20, 15, 15),
            new LockoutTier(50, 60, 60),
        };

        public LoginLockoutStatus CheckLockout(IQueryable<LoginAttempt> attempts, int serverId, string ipHash, string usernameKey, DateTime nowUtc)
        {
            DateTime? retryAfter = EvaluateTiers(attempts.Where(a => a.ServerId == serverId && a.UsernameKey == usernameKey), AccountTiers, nowUtc);
            if (!string.IsNullOrEmpty(ipHash))
            {
                var ipRetryAfter = EvaluateTiers(attempts.Where(a => a.ServerId == serverId && a.IpHash == ipHash), IpTiers, nowUtc);
                if (ipRetryAfter.HasValue && (!retryAfter.HasValue || ipRetryAfter.Value > retryAfter.Value)) retryAfter = ipRetryAfter;
            }
            return new LoginLockoutStatus { IsLockedOut = retryAfter.HasValue, RetryAfterUtc = retryAfter };
        }

        private static DateTime? EvaluateTiers(IQueryable<LoginAttempt> scoped, LockoutTier[] tiers, DateTime nowUtc)
        {
            var maxWindowMinutes = tiers.Max(t => t.WindowMinutes);
            var failedTimestamps = scoped
                .Where(a => !a.Succeeded && a.AttemptedAtUtc >= nowUtc.AddMinutes(-maxWindowMinutes))
                .Select(a => a.AttemptedAtUtc)
                .ToList();

            DateTime? strictest = null;
            foreach (var tier in tiers)
            {
                var windowStart = nowUtc.AddMinutes(-tier.WindowMinutes);
                var inWindow = failedTimestamps.Where(t => t >= windowStart).ToList();
                if (inWindow.Count < tier.Attempts) continue;
                var retryAfter = inWindow.Max().AddMinutes(tier.LockoutMinutes);
                if (retryAfter > nowUtc && (!strictest.HasValue || retryAfter > strictest.Value)) strictest = retryAfter;
            }
            return strictest;
        }

        public LoginAttempt RecordAttempt(int serverId, string ipHash, string usernameKey, bool succeeded, DateTime nowUtc)
        {
            return new LoginAttempt { ServerId = serverId, IpHash = ipHash, UsernameKey = usernameKey, Succeeded = succeeded, AttemptedAtUtc = nowUtc };
        }

        public static string NormalizeUsernameKey(string userNameOrEmail)
        {
            return (userNameOrEmail ?? string.Empty).Trim().ToLowerInvariant();
        }

        public static string HashIp(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress)) return "unknown";
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(ipAddress));
                return Convert.ToHexString(bytes);
            }
        }
    }
}
