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
        private readonly PlanetMarketService _planetMarket;
        private readonly ShipyardService _shipyard;
        private readonly FleetService _fleets;
        private readonly EspionageService _espionage;

        public GameController(GameDbContext db, EconomyService economy, BuildingCatalogService catalog, BuildQueueService buildQueue, ResourceService resources, ResearchCatalogService researchCatalog, ResearchQueueService researchQueue, FactionModifierService factionModifiers, GateMissionService gateMissions, LocalSectorService localSectors, PlanetMarketService planetMarket, ShipyardService shipyard, FleetService fleets, EspionageService espionage)
        {
            _db = db; _economy = economy; _catalog = catalog; _buildQueue = buildQueue; _resources = resources; _researchCatalog = researchCatalog; _researchQueue = researchQueue; _factionModifiers = factionModifiers; _gateMissions = gateMissions; _localSectors = localSectors; _planetMarket = planetMarket; _shipyard = shipyard; _fleets = fleets; _espionage = espionage;
        }

        public IActionResult Overview() { return GameView("Overview"); }
        public IActionResult Base() { return GameView("Base"); }
        public IActionResult Planet() { return GameView("Planet"); }
        public IActionResult Sectors() { return GameView("Sectors"); }
        public IActionResult Resources() { return GameView("Resources"); }
        public IActionResult Reports() { return GameView("Reports"); }
        public IActionResult Research() { return GameView("Research"); }
        public IActionResult GateRoom() { return GameView("GateRoom"); }
        public IActionResult Market() { return GameView("Market"); }
        public IActionResult Shipyard() { return GameView("Shipyard"); }
        public IActionResult Ships() { return GameView("Ships"); }
        public IActionResult SendFleet() { return GameView("SendFleet"); }
        public IActionResult Fleets() { return GameView("Fleets"); }
        public IActionResult FleetReports() { return GameView("FleetReports"); }
        public IActionResult Orbit() { return GameView("Orbit"); }
        public IActionResult Intelligence() { return GameView("Intelligence"); }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartEspionage(int targetBaseId, EspionageMissionType missionType, int intelSpent)
        {
            var source = LoadCurrentBase();
            if (source == null) return RedirectToAction("Login", "Account");
            var user = LoadCurrentUser(source.UserId);
            var now = DateTime.UtcNow;
            _economy.ApplyOfflineProduction(source, now);
            try
            {
                var target = _db.PlayerBases.Include(b => b.User).ThenInclude(u => u.ResearchLevels).Include(b => b.Faction).Include(b => b.Resources).Include(b => b.BuildingLevels).Include(b => b.Ships).Include(b => b.PlanetSector).ThenInclude(s => s.Planet).Single(b => b.Id == targetBaseId);
                var mission = _espionage.StartMission(user, source, target, missionType, intelSpent, now);
                _db.EspionageMissions.Add(mission);
                var sectors = _db.PlanetSectors.Include(s => s.SectorControl).Where(s => s.PlanetId == target.PlanetSector.PlanetId && s.SectorControl != null && s.SectorControl.UserId == target.UserId).ToList();
                var markets = _db.PlanetMarketOrders.Where(o => o.PlanetId == target.PlanetSector.PlanetId && o.SellerUserId == target.UserId && o.CompletedAtUtc == null && o.CancelledAtUtc == null).ToList();
                var fleets = _db.FleetMovements.Where(f => f.UserId == target.UserId && f.Status != FleetMovementStatus.Completed).ToList();
                _db.IntelligenceReports.Add(_espionage.CreateReport(mission, sectors, markets, fleets, now));
                if (mission.WasDetected)
                {
                    _db.IntelligenceReports.Add(new IntelligenceReport { UserId = target.UserId, CreatedAtUtc = now, Title = "Spionagewarnung", Body = "Gegenspionage meldet eine " + mission.MissionType + " gegen " + target.Name + ". Risikoauswertung: " + mission.DetectionRiskPercent + "%.", DetailDepth = 1, IsWarning = true, WasDetected = true });
                }
                TempData["Message"] = "Spionagemission abgeschlossen. In Version 0.0.8 werden keine direkten Angriffe ausgelöst.";
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException || ex is ArgumentOutOfRangeException)
            {
                TempData["Error"] = ex.Message;
            }
            _db.SaveChanges();
            return RedirectToAction("Intelligence");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartShipBuild(ShipType shipType, int quantity)
        {
            var playerBase = LoadCurrentBase(); if (playerBase == null) return RedirectToAction("Login", "Account");
            var now = DateTime.UtcNow; _economy.ApplyOfflineProduction(playerBase, now);
            try { _shipyard.StartBuild(playerBase, shipType, quantity, now); TempData["Message"] = "Schiffsbau gestartet."; }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException || ex is ArgumentOutOfRangeException) { TempData["Error"] = ex.Message; }
            _db.SaveChanges(); return RedirectToAction("Shipyard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartFleet(int targetBaseId, FleetMissionType missionType, ShipType shipType, int shipCount, int naquadah, int trinium, int supplies, int energy, int personnel)
        {
            var origin = LoadCurrentBase(); if (origin == null) return RedirectToAction("Login", "Account");
            var now = DateTime.UtcNow; _economy.ApplyOfflineProduction(origin, now);
            try { var target = _db.PlayerBases.Include(b=>b.Resources).Include(b=>b.Ships).Include(b=>b.PlanetSector).ThenInclude(s=>s.Planet).Single(b=>b.Id==targetBaseId); if(target.UserId!=origin.UserId && target.PlanetSector.PlanetId!=origin.PlanetSector.PlanetId && !_db.KnownGateAddresses.Include(k=>k.GateAddress).Any(k=>k.UserId==origin.UserId && k.GateAddress.PlanetId==target.PlanetSector.PlanetId)) throw new InvalidOperationException("Ziel ist weder auf gleichem Planeten noch als bekanntes Ziel freigeschaltet."); var cargo=new ResourceStock{Naquadah=Math.Max(0,naquadah),Trinium=Math.Max(0,trinium),Supplies=Math.Max(0,supplies),Energy=Math.Max(0,energy),Personnel=Math.Max(0,personnel)}; var fleet=_fleets.Start(origin,target,missionType,shipType,shipCount,cargo,now); _db.FleetMovements.Add(fleet); TempData["Message"]="Flotte gestartet. Schiffe nutzen Hyperraum oder lokalen Raumflug, nicht das Stargate."; }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException || ex is ArgumentOutOfRangeException) { TempData["Error"] = ex.Message; }
            _db.SaveChanges(); return RedirectToAction("Fleets");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CompleteFleet(int fleetId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId"); if (!userId.HasValue) return RedirectToAction("Login", "Account"); var now=DateTime.UtcNow;
            try { var fleet=_db.FleetMovements.Include(f=>f.OriginBase).ThenInclude(b=>b.Ships).Include(f=>f.OriginBase).ThenInclude(b=>b.Resources).Include(f=>f.OriginBase).ThenInclude(b=>b.PlanetSector).Include(f=>f.TargetBase).ThenInclude(b=>b.Ships).Include(f=>f.TargetBase).ThenInclude(b=>b.Resources).Include(f=>f.TargetBase).ThenInclude(b=>b.PlanetSector).Single(f=>f.Id==fleetId && f.UserId==userId.Value); var report=_fleets.Complete(fleet,now); _db.FleetReports.Add(report); TempData["Message"]="Flottenereignis abgeschlossen."; }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException) { TempData["Error"] = ex.Message; }
            _db.SaveChanges(); return RedirectToAction("Fleets");
        }

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
        public IActionResult CreateMarketOrder(TradeResourceType offeredResource, int offeredAmount, TradeResourceType requestedResource, int requestedAmount, int durationHours)
        {
            var playerBase = LoadCurrentBase();
            if (playerBase == null) return RedirectToAction("Login", "Account");
            var user = LoadCurrentUser(playerBase.UserId);
            var now = DateTime.UtcNow;
            _economy.ApplyOfflineProduction(playerBase, now);
            try
            {
                durationHours = Math.Max(1, Math.Min(168, durationHours));
                var order = _planetMarket.CreateOrder(user, playerBase, offeredResource, offeredAmount, requestedResource, requestedAmount, now.AddHours(durationHours), now);
                _db.PlanetMarketOrders.Add(order);
                _db.TradeReports.Add(new TradeReport { UserId = user.Id, PlanetMarketOrder = order, CreatedAtUtc = now, Title = "Marktangebot erstellt", Body = "Deine Ressourcen wurden reserviert." });
                TempData["Message"] = "Marktangebot erstellt.";
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException || ex is ArgumentOutOfRangeException)
            {
                TempData["Error"] = ex.Message;
            }
            _db.SaveChanges();
            return RedirectToAction("Market");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BuyMarketOrder(int orderId)
        {
            var buyerBase = LoadCurrentBase();
            if (buyerBase == null) return RedirectToAction("Login", "Account");
            var buyer = LoadCurrentUser(buyerBase.UserId);
            var now = DateTime.UtcNow;
            using (var tx = _db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    var order = _db.PlanetMarketOrders.Include(o => o.SellerUser).ThenInclude(u => u.Faction).Single(o => o.Id == orderId);
                    var sellerBase = _db.PlayerBases.Include(b => b.User).ThenInclude(u => u.ResearchLevels).Include(b => b.Resources).Include(b => b.BuildingLevels).Include(b => b.Faction).Include(b => b.PlanetSector).Single(b => b.Id == order.SellerBaseId);
                    _economy.ApplyOfflineProduction(buyerBase, now);
                    _economy.ApplyOfflineProduction(sellerBase, now);
                    var sellerSectors = _db.PlanetSectors.Include(s => s.SectorControl).Where(s => s.PlanetId == order.PlanetId && s.SectorControl != null && s.SectorControl.UserId == order.SellerUserId).ToList();
                    var transaction = _planetMarket.BuyOrder(order, buyer, buyerBase, sellerBase, sellerSectors, now);
                    _db.PlanetMarketTransactions.Add(transaction);
                    _db.TradeReports.Add(new TradeReport { UserId = buyer.Id, PlanetMarketOrder = order, CreatedAtUtc = now, Title = "Marktangebot gekauft", Body = "Du hast das Angebot gekauft." });
                    _db.TradeReports.Add(new TradeReport { UserId = order.SellerUserId, PlanetMarketOrder = order, CreatedAtUtc = now, Title = "Marktangebot verkauft", Body = "Dein Angebot wurde gekauft. Marktgebühr: " + transaction.FeeAmount + "." });
                    _db.SaveChanges();
                    tx.Commit();
                    TempData["Message"] = "Marktangebot gekauft.";
                }
                catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException || ex is ArgumentOutOfRangeException || ex is DbUpdateException)
                {
                    tx.Rollback();
                    TempData["Error"] = ex.Message;
                }
            }
            return RedirectToAction("Market");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelMarketOrder(int orderId)
        {
            var playerBase = LoadCurrentBase();
            if (playerBase == null) return RedirectToAction("Login", "Account");
            var now = DateTime.UtcNow;
            try
            {
                var order = _db.PlanetMarketOrders.Single(o => o.Id == orderId && o.SellerUserId == playerBase.UserId);
                _planetMarket.CancelOrder(order, playerBase.UserId, playerBase.Resources, now);
                _db.TradeReports.Add(new TradeReport { UserId = playerBase.UserId, PlanetMarketOrder = order, CreatedAtUtc = now, Title = "Marktangebot storniert", Body = "Reservierte Ressourcen wurden zurückgegeben." });
                TempData["Message"] = "Marktangebot storniert.";
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                TempData["Error"] = ex.Message;
            }
            _db.SaveChanges();
            return RedirectToAction("Market");
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
            _shipyard.CompleteFinishedBuilds(playerBase, now);
            _researchQueue.CompleteFinishedResearch(user, now);
            var offlineBonus = _localSectors.CalculateBonus(_db.PlanetSectors.Include(s => s.SectorControl).Where(s => s.PlanetId == playerBase.PlanetSector.PlanetId && s.SectorControl != null && s.SectorControl.UserId == user.Id).ToList());
            _economy.ApplyOfflineProduction(playerBase, now, offlineBonus);
            ExpirePlanetMarketOrders(playerBase.PlanetSector.PlanetId, now);
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
            var model = new OverviewViewModel { User = user, Base = playerBase, Planet = planet, Hourly = _economy.CalculateHourlyProduction(playerBase.BuildingLevels, user.ResearchLevels, user.Faction, sectorBonus), Sectors = planet.Sectors.OrderBy(s => s.Number).ToList(), Reports = _db.Reports.Where(r => r.UserId == user.Id).OrderByDescending(r => r.CreatedAtUtc).ToList(), Buildings = buildings, ActiveBuild = playerBase.BuildQueue.OrderBy(q => q.CompletesAtUtc).FirstOrDefault(), NowUtc = now, Researches = BuildResearchViewModels(user, playerBase), ActiveResearch = user.ResearchQueue.OrderBy(q => q.CompletesAtUtc).FirstOrDefault(), DefenseModifier = _factionModifiers.GetDefenseMultiplier(user.Faction), KnownGateAddresses = _db.KnownGateAddresses.Include(k => k.GateAddress).Where(k => k.UserId == user.Id).ToList(), MissionTeams = _db.MissionTeams.Where(t => t.UserId == user.Id).ToList(), ActiveGateMissions = _db.GateMissions.Include(m => m.GateAddress).Include(m => m.MissionTeam).Where(m => m.UserId == user.Id && !m.IsCompleted).ToList(), GateMissionReports = _db.GateMissionReports.Include(r => r.GateMission).ThenInclude(m => m.GateAddress).Where(r => r.UserId == user.Id).OrderByDescending(r => r.CreatedAtUtc).ToList(), ActiveSectorClaims = activeSectorClaims, ControlledSectors = controlledSectors, SectorBonus = sectorBonus, PlanetInfluences = BuildPlanetInfluences(planet.Id), OwnInfluence = _localSectors.CalculateInfluence(playerBase, user, controlledSectors, activeSectorClaims.Where(c => c.UserId == user.Id)), ActiveMarketOrders = _db.PlanetMarketOrders.Include(o => o.SellerUser).Where(o => o.PlanetId == planet.Id && o.CompletedAtUtc == null && o.CancelledAtUtc == null && !o.ReservedReturned && o.ExpiresAtUtc > now).OrderBy(o => o.ExpiresAtUtc).ToList(), OwnMarketOrders = _db.PlanetMarketOrders.Where(o => o.PlanetId == planet.Id && o.SellerUserId == user.Id).OrderByDescending(o => o.CreatedAtUtc).ToList(), TradeReports = _db.TradeReports.Where(r => r.UserId == user.Id).OrderByDescending(r => r.CreatedAtUtc).ToList(), ShipDefinitions = BuildShipViewModels(user, playerBase), ActiveShipBuild = playerBase.ShipyardQueue.OrderBy(q => q.CompletesAtUtc).FirstOrDefault(), FleetTargets = _db.PlayerBases.Include(b=>b.User).Include(b=>b.PlanetSector).ThenInclude(s=>s.Planet).Where(b=>b.Id!=playerBase.Id).ToList(), ActiveFleets = _db.FleetMovements.Include(f=>f.TargetBase).ThenInclude(b=>b.PlanetSector).Where(f=>f.UserId==user.Id && f.Status!=FleetMovementStatus.Completed).ToList(), FleetReports = _db.FleetReports.Where(r=>r.UserId==user.Id).OrderByDescending(r=>r.CreatedAtUtc).ToList(), OrbitPresences = BuildOrbitPresences(planet.Id), EspionageTargets = _db.PlayerBases.Include(b=>b.User).Include(b=>b.Faction).Include(b=>b.PlanetSector).ThenInclude(s=>s.Planet).Where(b=>b.Id!=playerBase.Id).ToList(), IntelligenceReports = _db.IntelligenceReports.Where(r=>r.UserId==user.Id && !r.IsWarning).OrderByDescending(r=>r.CreatedAtUtc).ToList(), SpyWarnings = _db.IntelligenceReports.Where(r=>r.UserId==user.Id && r.IsWarning).OrderByDescending(r=>r.CreatedAtUtc).ToList() };
            return View(view, model);
        }


        private void ExpirePlanetMarketOrders(int planetId, DateTime now)
        {
            var expired = _db.PlanetMarketOrders.Where(o => o.PlanetId == planetId && o.CompletedAtUtc == null && o.CancelledAtUtc == null && !o.ReservedReturned && o.ExpiresAtUtc <= now).ToList();
            foreach (var order in expired)
            {
                var sellerBase = _db.PlayerBases.Include(b => b.Resources).Single(b => b.Id == order.SellerBaseId);
                if (_planetMarket.ExpireOrder(order, sellerBase.Resources, now))
                {
                    _db.TradeReports.Add(new TradeReport { UserId = order.SellerUserId, PlanetMarketOrder = order, CreatedAtUtc = now, Title = "Marktangebot abgelaufen", Body = "Reservierte Ressourcen wurden zurückgegeben." });
                }
            }
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
            return _db.PlayerBases.Include(b => b.User).ThenInclude(u => u.ResearchLevels).Include(b => b.Faction).Include(b => b.Resources).Include(b => b.BuildingLevels).Include(b => b.BuildQueue).Include(b => b.Ships).Include(b => b.ShipyardQueue).Include(b => b.PlanetSector).ThenInclude(s => s.Planet).First(b => b.UserId == sessionUserId.Value);
        }

        private User LoadCurrentUser(int userId)
        {
            var user = _db.Users.Include(u => u.Faction).Include(u => u.ResearchLevels).Include(u => u.ResearchQueue).First(u => u.Id == userId);
            if (user.ResearchLevels == null) user.ResearchLevels = new ResearchLevels { UserId = user.Id };
            return user;
        }


        private System.Collections.Generic.IList<ShipViewModel> BuildShipViewModels(User user, PlayerBase playerBase)
        {
            return _shipyard.GetAvailableForFaction(user.Faction).Select(d => new ShipViewModel { Type = d.Type, Name = d.Name, Cost = d.Cost, CargoCapacity = d.CargoCapacity, Speed = d.Speed, IsActive = d.IsActive, CanBuild = d.IsActive && playerBase.BuildingLevels.HangarLandingZone >= 1 && _resources.HasEnough(playerBase.Resources, d.Cost), Available = playerBase.Ships.GetCount(d.Type) }).ToList();
        }
        private System.Collections.Generic.IList<OrbitPresence> BuildOrbitPresences(int planetId)
        {
            return _db.PlayerBases.Include(b=>b.User).Include(b=>b.Ships).Include(b=>b.PlanetSector).ThenInclude(s=>s.Planet).Where(b=>b.PlanetSector.PlanetId==planetId).ToList().Select(b=>new OrbitPresence{PlanetId=planetId,PlanetName=b.PlanetSector.Planet.Name,UserId=b.UserId,UserName=b.User.UserName,StationedShips=b.Ships.F302+b.Ships.SmallTransporter+b.Ships.SupplyShuttle+b.Ships.Teltak+b.Ships.JaffaTransporter+b.Ships.CloakedTeltak+b.Ships.AgentTransporter+b.Ships.SmugglerTransporter+b.Ships.PirateFighter,MovingFleets=_db.FleetMovements.Count(f=>f.UserId==b.UserId&&f.Status!=FleetMovementStatus.Completed)}).ToList();
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
