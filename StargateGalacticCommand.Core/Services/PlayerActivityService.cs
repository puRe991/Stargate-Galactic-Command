using System;
using System.Linq;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class PlayerActivityService
    {
        public static readonly TimeSpan ActiveWindow = TimeSpan.FromMinutes(5);

        public void MarkSeen(User user, DateTime nowUtc)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            user.LastSeenAtUtc = nowUtc;
        }

        public int CountActiveHumanPlayers(IQueryable<User> users, DateTime nowUtc)
        {
            if (users == null) throw new ArgumentNullException(nameof(users));

            var cutoffUtc = nowUtc.Subtract(ActiveWindow);
            return users.Count(u => !u.IsNpc && u.LastSeenAtUtc >= cutoffUtc);
        }
    }
}
