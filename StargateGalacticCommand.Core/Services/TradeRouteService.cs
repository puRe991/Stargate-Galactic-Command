using System;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class TradeRouteService
    {
        public const int MinIntervalHours = 2;
        public const int MaxIntervalHours = 168;
        public const int MaxActiveRoutesPerUser = 5;

        public TradeRoute CreateRoute(User user, PlayerBase origin, PlayerBase target, ShipType shipType, int shipCount, ResourceStock cargo, int intervalHours, int activeRouteCount, DateTime nowUtc)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (origin == null) throw new ArgumentNullException("origin");
            if (target == null) throw new ArgumentNullException("target");
            if (origin.Id == target.Id) throw new InvalidOperationException("Ziel- und Herkunftsbasis müssen unterschiedlich sein.");
            if (shipCount < 1) throw new ArgumentOutOfRangeException("shipCount");
            if (intervalHours < MinIntervalHours || intervalHours > MaxIntervalHours) throw new ArgumentOutOfRangeException("intervalHours", "Intervall muss zwischen " + MinIntervalHours + " und " + MaxIntervalHours + " Stunden liegen.");
            if (activeRouteCount >= MaxActiveRoutesPerUser) throw new InvalidOperationException("Maximal " + MaxActiveRoutesPerUser + " aktive Handelsrouten gleichzeitig.");
            int load = (cargo?.Naquadah ?? 0) + (cargo?.Trinium ?? 0) + (cargo?.Supplies ?? 0) + (cargo?.Energy ?? 0) + (cargo?.Personnel ?? 0);
            if (load < 1) throw new InvalidOperationException("Eine Handelsroute muss mindestens eine Ressourcenart transportieren.");

            return new TradeRoute
            {
                UserId = user.Id,
                OriginBaseId = origin.Id,
                OriginBase = origin,
                TargetBaseId = target.Id,
                TargetBase = target,
                ShipType = shipType,
                ShipCount = shipCount,
                Naquadah = Math.Max(0, cargo?.Naquadah ?? 0),
                Trinium = Math.Max(0, cargo?.Trinium ?? 0),
                Supplies = Math.Max(0, cargo?.Supplies ?? 0),
                Energy = Math.Max(0, cargo?.Energy ?? 0),
                Personnel = Math.Max(0, cargo?.Personnel ?? 0),
                IntervalHours = intervalHours,
                IsActive = true,
                NextDueAtUtc = nowUtc,
                CreatedAtUtc = nowUtc
            };
        }

        public bool IsDue(TradeRoute route, DateTime nowUtc)
        {
            return route != null && route.IsActive && nowUtc >= route.NextDueAtUtc;
        }

        public void MarkExecuted(TradeRoute route, DateTime nowUtc)
        {
            if (route == null) throw new ArgumentNullException("route");
            route.LastExecutedAtUtc = nowUtc;
            route.NextDueAtUtc = nowUtc.AddHours(route.IntervalHours);
        }

        public void Pause(TradeRoute route)
        {
            if (route == null) throw new ArgumentNullException("route");
            route.IsActive = false;
        }

        public void Resume(TradeRoute route, DateTime nowUtc)
        {
            if (route == null) throw new ArgumentNullException("route");
            route.IsActive = true;
            route.NextDueAtUtc = nowUtc;
        }
    }
}
