using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using StargateGalacticCommand.Data;
using StargateGalacticCommand.Web.Models;

namespace StargateGalacticCommand.Web.Controllers
{
    public class GameController : Controller
    {
        private readonly GameDbContext _db;
        private readonly EconomyService _economy;
        private readonly BuildingCatalogService _catalog;
        private readonly BuildQueueService _buildQueue;
        private readonly ResourceService _resources;
        private readonly ResearchCatalogService _researchCatalog;
        private readonly ResearchQueueService _researchQueue;
        private readonly FactionModifierService _factionModifiers;
        private readonly GateMissionService _gateMissions;
        private readonly LocalSectorService _localSectors;

        public GameController(GameDbContext db, EconomyService economy, BuildingCatalogService catalog, BuildQueueService buildQueue, ResourceService resources, ResearchCatalogService researchCatalog, ResearchQueueService researchQueue, FactionModifierService factionModifiers, GateMissionService gateMissions, LocalSectorService localSectors)
        {
            _db = db; _economy = economy; _catalog = catalog; _buildQueue = buildQueue; _resources = resources; _researchCatalog = researchCatalog; _researchQueue = researchQueue; _factionModifiers = factionModifiers; _gateMissions = gateMissions; _localSectors = localSectors;
        }

        public IActionResult Overview() { return GameView("Overview"); }
        public IActionResult Base() { return GameView("Base"); }
        public IActionResult Planet() { return GameView("Planet"); }
        public IActionResult Sectors() { return GameView("Sectors"); }
        public IActionResult Resources() { return GameView("Resources"); }
        public IActionResult Reports() { return GameView("Reports"); }
        public IActionResult Research() { return GameView("Research"); }
        public IActionResult GateRoom() { return GameView("GateRoom"); }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClaimSector(int sectorId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");
            var user = LoadCurrentUser(userId.Value);
            var playerBase = LoadCurrentBase(user.Id);
            var now = DateTime.UtcNow;
            try
            {
                var sector = _db.PlanetSectors.Include(s => s.PlayerBase).Include(s => s.SectorControl).Single(s => s.Id == sectorId && s.PlanetId == playerBase.PlanetSector.PlanetId);
                var activeClaims = _db.SectorClaims.Where(c => !c.IsCompleted && c.PlanetSectorId == sector.Id).ToList();
                _db.SectorClaims.Add(_localSectors.StartClaim(user, sector, activeClaims, now));
                TempData["Message"] = "Beanspruchung gestartet. Der Startplanet bleibt PvP-geschützt; Angriffe sind in Version 0.0.5 deaktiviert.";
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                TempData["Error"] = ex.Message;
            }
            _db.SaveChanges();
            return RedirectToAction("Planet");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CompleteSectorClaim(int claimId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");
            var now = DateTime.UtcNow;
            try
            {
                var claim = _db.SectorClaims.Include(c => c.PlanetSector).ThenInclude(s => s.SectorControl).Single(c => c.Id == claimId && c.UserId == userId.Value);
                var report = _localSectors.CompleteClaim(claim, now);
                _db.LocalActionReports.Add(report);
                _db.Reports.Add(new Report { UserId = userId.Value, Title = report.Title, Body = report.Body, CreatedAtUtc = now });
                TempData["Message"] = "Sektor erfolgreich gesichert.";
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                TempData["Error"] = ex.Message;
            }
            _db.SaveChanges();
            return RedirectToAction("Planet");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartGateMission(int gateAddressId, int missionTeamId, GateMissionType missionType)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");
            var user = LoadCurrentUser(userId.Value);
            EnsureGateAccessForUser(user);
            var playerBase = LoadCurrentBase(user.Id);
            var now = DateTime.UtcNow;
            _economy.ApplyOfflineProduction(playerBase, now);
            try
            {
                bool knowsAddress = _db.KnownGateAddresses.Any(k => k.UserId == user.Id && k.GateAddressId == gateAddressId);
                if (!knowsAddress) throw new InvalidOperationException("Gate-Adresse ist nicht bekannt.");
                var address = _db.GateAddresses.Include(a => a.Planet).Single(a => a.Id == gateAddressId);
                var team = _db.MissionTeams.Single(t => t.Id == missionTeamId && t.UserId == user.Id);
                var mission = _gateMissions.StartMission(user, playerBase, address, team, missionType, now);
                _db.GateMissions.Add(mission);
                TempData["Message"] = "Gate-Mission gestartet. Keine Schiffe oder Großflotten passieren das Gate.";
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException || ex is ArgumentOutOfRangeException)
            {
                TempData["Error"] = ex.Message;
            }
            _db.SaveChanges();
            return RedirectToAction("GateRoom");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CompleteGateMission(int gateMissionId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");
            var playerBase = LoadCurrentBase(userId.Value);
            var now = DateTime.UtcNow;
            try
            {
                var mission = _db.GateMissions.Include(m => m.MissionTeam).Include(m => m.GateAddress).Single(m => m.Id == gateMissionId && m.UserId == userId.Value);
                var report = _gateMissions.CompleteMission(mission, playerBase, _db.GateAddresses.ToList(), now);
                _db.GateMissionReports.Add(report);
                if (mission.MissionType == GateMissionType.AnalyzeAddress && report.Outcome != GateMissionOutcome.WoundedOrLosses)
                {
                    var knownIds = _db.KnownGateAddresses.Where(k => k.UserId == userId.Value).Select(k => k.GateAddressId).ToList();
                    var nextAddress = _db.GateAddresses.Where(a => a.IsNeutralPve && !knownIds.Contains(a.Id)).OrderBy(a => a.RiskLevel).FirstOrDefault();
                    if (nextAddress != null)
                    {
                        _db.KnownGateAddresses.Add(new KnownGateAddress { UserId = userId.Value, GateAddressId = nextAddress.Id, DiscoveredAtUtc = now, DiscoveryMethod = "Adresse analysieren" });
                        report.Summary += " Neue Adresse freigeschaltet: " + nextAddress.Code + ".";
                    }
                }
                _db.Reports.Add(new Report { UserId = userId.Value, Title = "Gate-Mission: " + mission.MissionType, Body = report.Summary, CreatedAtUtc = now });
                TempData["Message"] = "Gate-Mission abgeschlossen.";
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                TempData["Error"] = ex.Message;
            }
            _db.SaveChanges();
            return RedirectToAction("GateRoom");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartBuild(BuildingType buildingType)
        {
            var playerBase = LoadCurrentBase();
            if (playerBase == null) return RedirectToAction("Login", "Account");
            var now = DateTime.UtcNow;
            _economy.ApplyOfflineProduction(playerBase, now);
            try
            {
                _buildQueue.StartBuild(playerBase, buildingType, now);
                TempData["Message"] = "Ausbau gestartet.";
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException || ex is ArgumentOutOfRangeException)
            {
                TempData["Error"] = ex.Message;
            }
            _db.SaveChanges();
            return RedirectToAction("Base");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartResearch(ResearchType researchType)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");
            var user = LoadCurrentUser(userId.Value);
            var playerBase = LoadCurrentBase(user.Id);
            var now = DateTime.UtcNow;
            _economy.ApplyOfflineProduction(playerBase, now);
            try
            {
                _researchQueue.StartResearch(user, playerBase, researchType, now);
                TempData["Message"] = "Forschung gestartet.";
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException || ex is ArgumentOutOfRangeException)
            {
                TempData["Error"] = ex.Message;
            }
            _db.SaveChanges();
            return RedirectToAction("Research");
        }

        private IActionResult GameView(string view)
        {
            int? userId = HttpContext.Session.GetInt32("UserId"); if (!userId.HasValue) return RedirectToAction("Login", "Account");
            var user = LoadCurrentUser(userId.Value);
            EnsureGateAccessForUser(user);
            var playerBase = LoadCurrentBase(user.Id);
            var now = DateTime.UtcNow;
            _buildQueue.CompleteFinishedBuilds(playerBase, now);
            _researchQueue.CompleteFinishedResearch(user, now);
            var offlineBonus = _localSectors.CalculateBonus(_db.PlanetSectors.Include(s => s.SectorControl).Where(s => s.PlanetId == playerBase.PlanetSector.PlanetId && s.SectorControl != null && s.SectorControl.UserId == user.Id).ToList());
            _economy.ApplyOfflineProduction(playerBase, now, offlineBonus);
            _db.SaveChanges();
            var planet = _db.Planets.Include(p => p.Sectors).ThenInclude(s => s.PlayerBase).Include(p => p.Sectors).ThenInclude(s => s.SectorControl).Single(p => p.Id == playerBase.PlanetSector.PlanetId);
            var activeSectorClaims = _db.SectorClaims.Include(c => c.PlanetSector).Where(c => !c.IsCompleted && c.PlanetSector.PlanetId == planet.Id).ToList();
            var controlledSectors = planet.Sectors.Where(s => s.SectorControl != null && s.SectorControl.UserId == user.Id).ToList();
            var sectorBonus = _localSectors.CalculateBonus(controlledSectors);
            bool queueBusy = playerBase.BuildQueue.Any();
            var buildings = _catalog.GetAll().Select(d =>
            {
                int level = playerBase.BuildingLevels.GetLevel(d.Type);
                var cost = _catalog.CalculateCost(d.Type, level);
                return new BuildingUpgradeViewModel
                {
                    Type = d.Type,
                    Name = d.Name,
                    Level = level,
                    Cost = cost,
                    BuildSeconds = _catalog.CalculateBuildSeconds(d.Type, level, playerBase.BuildingLevels.CommandCenter),
                    CanAfford = _resources.HasEnough(playerBase.Resources, cost),
                    QueueBusy = queueBusy
                };
            }).ToList();
            var model = new OverviewViewModel { User = user, Base = playerBase, Planet = planet, Hourly = _economy.CalculateHourlyProduction(playerBase.BuildingLevels, user.ResearchLevels, user.Faction, sectorBonus), Sectors = planet.Sectors.OrderBy(s => s.Number).ToList(), Reports = _db.Reports.Where(r => r.UserId == user.Id).OrderByDescending(r => r.CreatedAtUtc).ToList(), Buildings = buildings, ActiveBuild = playerBase.BuildQueue.OrderBy(q => q.CompletesAtUtc).FirstOrDefault(), NowUtc = now, Researches = BuildResearchViewModels(user, playerBase), ActiveResearch = user.ResearchQueue.OrderBy(q => q.CompletesAtUtc).FirstOrDefault(), DefenseModifier = _factionModifiers.GetDefenseMultiplier(user.Faction), KnownGateAddresses = _db.KnownGateAddresses.Include(k => k.GateAddress).Where(k => k.UserId == user.Id).ToList(), MissionTeams = _db.MissionTeams.Where(t => t.UserId == user.Id).ToList(), ActiveGateMissions = _db.GateMissions.Include(m => m.GateAddress).Include(m => m.MissionTeam).Where(m => m.UserId == user.Id && !m.IsCompleted).ToList(), GateMissionReports = _db.GateMissionReports.Include(r => r.GateMission).ThenInclude(m => m.GateAddress).Where(r => r.UserId == user.Id).OrderByDescending(r => r.CreatedAtUtc).ToList(), ActiveSectorClaims = activeSectorClaims, ControlledSectors = controlledSectors, SectorBonus = sectorBonus, PlanetInfluences = BuildPlanetInfluences(planet.Id), OwnInfluence = _localSectors.CalculateInfluence(playerBase, user, controlledSectors, activeSectorClaims.Where(c => c.UserId == user.Id)) };
            return View(view, model);
        }

        private System.Collections.Generic.IList<PlanetInfluence> BuildPlanetInfluences(int planetId)
        {
            var bases = _db.PlayerBases.Include(b => b.User).ThenInclude(u => u.ResearchLevels).Include(b => b.BuildingLevels).Include(b => b.PlanetSector).Where(b => b.PlanetSector.PlanetId == planetId).ToList();
            var sectors = _db.PlanetSectors.Include(s => s.SectorControl).Where(s => s.PlanetId == planetId && s.SectorControl != null).ToList();
            var claims = _db.SectorClaims.Include(c => c.PlanetSector).Where(c => !c.IsCompleted && c.PlanetSector.PlanetId == planetId).ToList();
            return bases.Select(b => new PlanetInfluence { PlanetId = planetId, UserId = b.UserId, UserName = b.User.UserName, Score = _localSectors.CalculateInfluence(b, b.User, sectors.Where(s => s.SectorControl.UserId == b.UserId), claims.Where(c => c.UserId == b.UserId)) }).OrderByDescending(i => i.Score).ToList();
        }

        private void EnsureGateAccessForUser(User user)
        {
            if (user == null) return;
            if (!_db.MissionTeams.Any(t => t.UserId == user.Id)) _db.MissionTeams.Add(_gateMissions.CreateFactionTeam(user));
            var start = _db.GateAddresses.SingleOrDefault(a => a.Code == "P3X-742");
            if (start != null && !_db.KnownGateAddresses.Any(k => k.UserId == user.Id && k.GateAddressId == start.Id))
                _db.KnownGateAddresses.Add(new KnownGateAddress { UserId = user.Id, GateAddressId = start.Id, DiscoveredAtUtc = DateTime.UtcNow, DiscoveryMethod = "Startplanet" });
        }

        private PlayerBase LoadCurrentBase(int? userId = null)
        {
            int? sessionUserId = userId ?? HttpContext.Session.GetInt32("UserId");
            if (!sessionUserId.HasValue) return null;
            return _db.PlayerBases.Include(b => b.User).ThenInclude(u => u.ResearchLevels).Include(b => b.Faction).Include(b => b.Resources).Include(b => b.BuildingLevels).Include(b => b.BuildQueue).Include(b => b.PlanetSector).ThenInclude(s => s.Planet).First(b => b.UserId == sessionUserId.Value);
        }

        private User LoadCurrentUser(int userId)
        {
            var user = _db.Users.Include(u => u.Faction).Include(u => u.ResearchLevels).Include(u => u.ResearchQueue).First(u => u.Id == userId);
            if (user.ResearchLevels == null) user.ResearchLevels = new ResearchLevels { UserId = user.Id };
            return user;
        }

        private System.Collections.Generic.IList<ResearchViewModel> BuildResearchViewModels(User user, PlayerBase playerBase)
        {
            bool queueBusy = user.ResearchQueue.Any();
            return _researchCatalog.GetAvailableForFaction(user.Faction).Select(d =>
            {
                int level = user.ResearchLevels.GetLevel(d.Type);
                var cost = _researchCatalog.CalculateCost(d.Type, level);
                bool prerequisiteMet = !d.Prerequisite.HasValue || user.ResearchLevels.GetLevel(d.Prerequisite.Value) > 0;
                bool hasLab = playerBase.BuildingLevels.ResearchLab >= 1;
                return new ResearchViewModel
                {
                    Type = d.Type, Name = d.Name, IsFactionResearch = d.FactionShortName != null, Level = level, Cost = cost,
                    ResearchSeconds = hasLab ? _researchCatalog.CalculateResearchSeconds(d.Type, level, playerBase.BuildingLevels.ResearchLab, _factionModifiers.GetResearchSpeedMultiplier(user.Faction)) : 0,
                    PrerequisiteName = d.Prerequisite.HasValue ? _researchCatalog.Get(d.Prerequisite.Value).Name : null,
                    PrerequisiteMet = prerequisiteMet, CanAfford = _resources.HasEnough(playerBase.Resources, cost), QueueBusy = queueBusy, HasResearchLab = hasLab
                };
            }).ToList();
        }
    }
}
