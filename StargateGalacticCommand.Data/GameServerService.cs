using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;

namespace StargateGalacticCommand.Data
{
    public class GameServerService
    {
        private readonly GameDbContext _db;
        private readonly GateMissionService _gateMissions;

        public GameServerService(GameDbContext db, GateMissionService gateMissions)
        {
            _db = db;
            _gateMissions = gateMissions;
        }

        public List<GameServer> ListServers()
        {
            return _db.GameServers.OrderBy(s => s.Id).ToList();
        }

        public GameServer GetServer(int id)
        {
            return _db.GameServers.SingleOrDefault(s => s.Id == id);
        }

        public GameServer CreateServer(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name fehlt.");
            var slug = Slugify(name);
            if (string.IsNullOrEmpty(slug)) throw new ArgumentException("Name muss lesbare Zeichen enthalten.");
            if (_db.GameServers.Any(s => s.Slug == slug)) throw new InvalidOperationException("Ein Server mit diesem Namen existiert bereits.");

            var server = new GameServer
            {
                Name = name.Trim(),
                Slug = slug,
                Description = (description ?? string.Empty).Trim(),
                Status = ServerStatus.Online,
                CreatedAtUtc = DateTime.UtcNow
            };
            _db.GameServers.Add(server);
            _db.SaveChanges();

            server.GalaxySeed = DatabaseInitializer.GalaxySeed + server.Id;
            DatabaseInitializer.SeedGalaxyForServer(_db, server.Id, server.GalaxySeed);
            _db.SaveChanges();
            return server;
        }

        public void StartServer(int id) => SetStatus(id, ServerStatus.Online);
        public void PauseServer(int id) => SetStatus(id, ServerStatus.Paused);
        public void StopServer(int id) => SetStatus(id, ServerStatus.Stopped);

        private void SetStatus(int id, ServerStatus status)
        {
            var server = _db.GameServers.SingleOrDefault(s => s.Id == id);
            if (server == null) throw new InvalidOperationException("Server nicht gefunden.");
            server.Status = status;
            _db.SaveChanges();
        }

        private static string Slugify(string name)
        {
            var sb = new StringBuilder();
            foreach (var c in (name ?? string.Empty).Trim().ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(c)) sb.Append(c);
                else if (sb.Length > 0 && sb[sb.Length - 1] != '-') sb.Append('-');
            }
            return sb.ToString().Trim('-');
        }
    }
}
