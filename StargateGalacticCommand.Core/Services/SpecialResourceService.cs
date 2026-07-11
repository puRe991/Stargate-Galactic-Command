using System;
using System.Linq;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class SpecialResourceService
    {
        public int GetQuantity(PlayerBase playerBase, SpecialResourceType type)
        {
            if (playerBase == null) throw new ArgumentNullException("playerBase");
            var entry = playerBase.SpecialResources.FirstOrDefault(r => r.Type == type);
            return entry == null ? 0 : entry.Quantity;
        }

        public PlayerBaseSpecialResource Add(PlayerBase playerBase, SpecialResourceType type, int amount)
        {
            if (playerBase == null) throw new ArgumentNullException("playerBase");
            if (amount < 0) throw new ArgumentOutOfRangeException("amount", "Amount must not be negative.");
            var entry = playerBase.SpecialResources.FirstOrDefault(r => r.Type == type);
            if (entry == null)
            {
                entry = new PlayerBaseSpecialResource { PlayerBaseId = playerBase.Id, PlayerBase = playerBase, Type = type, Quantity = 0 };
                playerBase.SpecialResources.Add(entry);
            }
            entry.Quantity += amount;
            return entry;
        }

        public bool HasEnough(PlayerBase playerBase, SpecialResourceType type, int amount)
        {
            return GetQuantity(playerBase, type) >= amount;
        }

        public void Spend(PlayerBase playerBase, SpecialResourceType type, int amount)
        {
            if (!HasEnough(playerBase, type, amount)) throw new InvalidOperationException("Not enough of the requested special resource.");
            var entry = playerBase.SpecialResources.First(r => r.Type == type);
            entry.Quantity -= amount;
        }
    }
}
