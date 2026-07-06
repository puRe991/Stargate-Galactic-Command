using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using StargateGalacticCommand.Data;
using StargateGalacticCommand.Web.Models;
using StargateGalacticCommand.Web.Filters;

namespace StargateGalacticCommand.Web.Controllers
{
    [RequireLogin]
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
        private readonly LocalCombatService _localCombat;
        private readonly AllianceService _alliances;
        private readonly SpaceCombatService _spaceCombat;
        private readonly RankingService _ranking;
        private readonly MessageService _messages;

        private static readonly TimeSpan OnlineWindow = TimeSpan.FromMinutes(15);

        public GameController(GameDbContext db, EconomyService economy, BuildingCatalogService catalog, BuildQueueService buildQueue, ResourceService resources, ResearchCatalogService researchCatalog, ResearchQueueService researchQueue, FactionModifierService factionModifiers, GateMissionService gateMissions, LocalSectorService localSectors, PlanetMarketService planetMarket, ShipyardService shipyard, FleetService fleets, EspionageService espionage, LocalCombatService localCombat, AllianceService alliances, SpaceCombatService spaceCombat, RankingService ranking, MessageService messages)
        {
            _db = db; _economy = economy; _catalog = catalog; _buildQueue = buildQueue; _resources = resources; _researchCatalog = researchCatalog; _researchQueue = researchQueue; _factionModifiers = factionModifiers; _gateMissions = gateMissions; _localSectors = localSectors; _planetMarket = planetMarket; _shipyard = shipyard; _fleets = fleets; _espionage = espionage; _localCombat = localCombat; _alliances = alliances; _spaceCombat = spaceCombat; _ranking = ranking; _messages = messages;
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
        public IActionResult Alliances() { return GameView("Alliances"); }
        public IActionResult CombatReports() { return GameView("CombatReports"); }
        public IActionResult Rankings() { return GameView("Rankings"); }
        public IActionResult Galaxy() { return GameView("Galaxy"); }
        public IActionResult Mailbox(int? to)
        {
            ViewData["PrefilledRecipientId"] = to;
            return GameView("Mailbox");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendMessage(int recipientUserId, string subject, string body)
        {
            int userId = CurrentUserId();
            var now = DateTime.UtcNow;
            try
            {
                var sender = LoadCurrentUser(userId);
                var recipient = _db.Users.Single(u => u.Id == recipientUserId);
                var message = _messages.Send(sender, recipient, subject, body, now);
                _db.PlayerMessages.Add(message);
                TempData["Message"] = "Nachricht gesendet.";
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                TempData["Error"] = ex.Message;
            }
            _db.SaveChanges();
            return RedirectToAction("Mailbox");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAlliance(string name, string tag, string description)
        {
            int userId = CurrentUserId();
            var now = DateTime.UtcNow;
            try { if (_db.AllianceMembers.Any(m => m.UserId == userId)) throw new InvalidOperationException("Du bist bereits in einer Allianz."); if (_db.Alliances.Any(a => a.Name == name || a.Tag == tag)) throw new InvalidOperationException("Name oder Tag ist bereits vergeben."); var alliance = _alliances.CreateAlliance(LoadCurrentUser(userId), name, tag, description, now); _db.Alliances.Add(alliance); _db.Reports.Add(new Report { UserId = userId, Title = "Allianz gegründet", Body = "Allianz " + alliance.Tag + " wurde erstellt.", CreatedAtUtc = now }); TempData["Message"] = "Allianz erstellt."; }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException) { TempData["Error"] = ex.Message; }
            _db.SaveChanges(); return RedirectToAction("Alliances");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApplyAlliance(int allianceId, string message)
        {
            int userId = CurrentUserId();
            var now = DateTime.UtcNow;
            try { var alliance = _db.Alliances.Single(a => a.Id == allianceId); var app = _alliances.Apply(alliance, LoadCurrentUser(userId), message, _db.AllianceMembers.Where(m => m.UserId == userId).ToList(), _db.AllianceApplications.Where(a => a.UserId == userId).ToList(), now); _db.AllianceApplications.Add(app); TempData["Message"] = "Bewerbung gesendet."; }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException) { TempData["Error"] = ex.Message; }
            _db.SaveChanges(); return RedirectToAction("Alliances");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AcceptAllianceApplication(int applicationId)
        {
            int userId = CurrentUserId();
            var now = DateTime.UtcNow;
            try { var app = _db.AllianceApplications.Single(a => a.Id == applicationId); var member = _alliances.Accept(app, LoadCurrentUser(userId), _db.AllianceMembers.Where(m => m.AllianceId == app.AllianceId).ToList(), now); _db.AllianceMembers.Add(member); _db.Reports.Add(new Report { UserId = app.UserId, Title = "Allianzbeitritt", Body = "Deine Bewerbung wurde angenommen.", CreatedAtUtc = now }); TempData["Message"] = "Bewerbung angenommen."; }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException) { TempData["Error"] = ex.Message; }
            _db.SaveChanges(); return RedirectToAction("Alliances");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartSpaceAttack(int targetBaseId, ShipType shipType, int shipCount)
        {
            var origin = LoadCurrentBase(); if (origin == null) return RedirectToAction("Login", "Account");
            var now = DateTime.UtcNow;
            try { var target = _db.PlayerBases.Include(b=>b.User).ThenInclude(u=>u.ResearchLevels).Include(b=>b.Faction).Include(b=>b.Resources).Include(b=>b.BuildingLevels).Include(b=>b.Ships).Include(b=>b.PlanetSector).ThenInclude(s=>s.Planet).Single(b=>b.Id==targetBaseId); var protection = _db.PlayerProtectionStatuses.SingleOrDefault(p=>p.UserId==target.UserId); var recentAttacksAgainstMe = _db.SpaceCombatMissions.Where(m=>m.AttackerUserId==target.UserId&&m.DefenderUserId==origin.UserId).ToList(); var mission = _spaceCombat.StartAttack(LoadCurrentUser(origin.UserId), origin, target, shipType, shipCount, _db.SpaceCombatMissions.Where(m=>m.AttackerUserId==origin.UserId&&m.TargetBaseId==target.Id).ToList(), protection, now, recentAttacksAgainstMe); _db.SpaceCombatMissions.Add(mission); _db.Reports.Add(_spaceCombat.BuildIncomingAttackReport(mission, now)); if(protection!=null) protection.LastAttackedAtUtc=now; TempData["Message"]="Angriff gestartet."; }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException || ex is ArgumentOutOfRangeException) { TempData["Error"] = ex.Message; }
            _db.SaveChanges(); return RedirectToAction("Fleets");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CompleteSpaceCombat(int missionId)
        {
            int userId = CurrentUserId(); var now=DateTime.UtcNow;
            try { var m=_db.SpaceCombatMissions.Include(x=>x.OriginBase).ThenInclude(b=>b.Ships).Include(x=>x.OriginBase).ThenInclude(b=>b.Resources).Include(x=>x.TargetBase).ThenInclude(b=>b.Ships).Include(x=>x.TargetBase).ThenInclude(b=>b.Resources).Include(x=>x.TargetBase).ThenInclude(b=>b.BuildingLevels).Single(x=>x.Id==missionId && x.AttackerUserId==userId); var attacker=LoadCurrentUser(m.AttackerUserId); var defender=_db.Users.Include(u=>u.Faction).Include(u=>u.ResearchLevels).Single(u=>u.Id==m.DefenderUserId); var report=_spaceCombat.Resolve(m,attacker,defender,now); _db.SpaceCombatReports.Add(report); _db.SpaceCombatReports.Add(new SpaceCombatReport{SpaceCombatMission=m,UserId=m.DefenderUserId,CreatedAtUtc=now,Title=report.Title,Body=report.Body,AttackerWon=report.AttackerWon,Rounds=report.Rounds,AttackerLosses=report.AttackerLosses,DefenderLosses=report.DefenderLosses}); _db.DebrisFields.Add(_spaceCombat.CreateDebris(m,report,now)); TempData["Message"]="Kampf berechnet."; }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException) { TempData["Error"] = ex.Message; }
            _db.SaveChanges(); return RedirectToAction("CombatReports");
        }

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
            int userId = CurrentUserId(); var now=DateTime.UtcNow;
            try { var fleet=_db.FleetMovements.Include(f=>f.OriginBase).ThenInclude(b=>b.Ships).Include(f=>f.OriginBase).ThenInclude(b=>b.Resources).Include(f=>f.OriginBase).ThenInclude(b=>b.PlanetSector).Include(f=>f.TargetBase).ThenInclude(b=>b.Ships).Include(f=>f.TargetBase).ThenInclude(b=>b.Resources).Include(f=>f.TargetBase).ThenInclude(b=>b.PlanetSector).Single(f=>f.Id==fleetId && f.UserId==userId); var report=_fleets.Complete(fleet,now); _db.FleetReports.Add(report); TempData["Message"]="Flottenereignis abgeschlossen."; }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException) { TempData["Error"] = ex.Message; }
            _db.SaveChanges(); return RedirectToAction("Fleets");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClaimSector(int sectorId)
        {
            int userId = CurrentUserId();
            var user = LoadCurrentUser(userId);
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
            int userId = CurrentUserId();
            var now = DateTime.UtcNow;
            try
            {
                var claim = _db.SectorClaims.Include(c => c.PlanetSector).ThenInclude(s => s.SectorControl).Single(c => c.Id == claimId && c.UserId == userId);
                var report = _localSectors.CompleteClaim(claim, now);
                _db.LocalActionReports.Add(report);
                _db.Reports.Add(new Report { UserId = userId, Title = report.Title, Body = report.Body, CreatedAtUtc = now });
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
        public IActionResult StartLocalCombat(int sectorId, LocalCombatObjective objective, int sgSecurityTeams, int marines, int jaffaWarriors, int eliteJaffa, int agentCells, int saboteurs, int mercenaries, int smugglerSquads)
        {
            int userId = CurrentUserId();
            var user = LoadCurrentUser(userId);
            var playerBase = LoadCurrentBase(user.Id);
            var now = DateTime.UtcNow;
            try
            {
                var sector = _db.PlanetSectors.Include(s => s.PlayerBase).Include(s => s.SectorControl).Single(s => s.Id == sectorId && s.PlanetId == playerBase.PlanetSector.PlanetId);
                var missions = _db.LocalCombatMissions.Where(m => m.PlanetSectorId == sector.Id).ToList();
                var defender = sector.SectorControl == null ? null : _db.Users.Include(u => u.ResearchLevels).Include(u => u.Faction).SingleOrDefault(u => u.Id == sector.SectorControl.UserId);
                var defenderBase = defender == null ? null : _db.PlayerBases.Include(b => b.BuildingLevels).FirstOrDefault(b => b.UserId == defender.Id);
                _localCombat.ValidateProtection(user, defender, _localSectors.CalculateInfluence(playerBase, user, null, null), _localSectors.CalculateInfluence(defenderBase, defender, null, null), now);
                var units = new GroundUnits { SgSecurityTeams = Math.Max(0, sgSecurityTeams), Marines = Math.Max(0, marines), JaffaWarriors = Math.Max(0, jaffaWarriors), EliteJaffa = Math.Max(0, eliteJaffa), AgentCells = Math.Max(0, agentCells), Saboteurs = Math.Max(0, saboteurs), Mercenaries = Math.Max(0, mercenaries), SmugglerSquads = Math.Max(0, smugglerSquads) };
                var mission = _localCombat.StartMission(user, playerBase, sector, objective, units, missions, now);
                _db.LocalCombatMissions.Add(mission);
                TempData["Message"] = "Lokaler Konflikt gestartet. Berechnung bei Ankunft.";
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException) { TempData["Error"] = ex.Message; }
            _db.SaveChanges();
            return RedirectToAction("Planet");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CompleteLocalCombat(int missionId)
        {
            int userId = CurrentUserId();
            var now = DateTime.UtcNow;
            try
            {
                var mission = _db.LocalCombatMissions.Include(m => m.AttackingUnits).Include(m => m.DefendingUnits).Include(m => m.PlanetSector).ThenInclude(s => s.PlayerBase).Include(m => m.PlanetSector).ThenInclude(s => s.SectorControl).Single(m => m.Id == missionId && m.AttackerUserId == userId);
                var attacker = _db.Users.Include(u => u.Faction).Single(u => u.Id == mission.AttackerUserId);
                var defender = mission.DefenderUserId.HasValue ? _db.Users.Include(u => u.Faction).SingleOrDefault(u => u.Id == mission.DefenderUserId.Value) : null;
                var report = _localCombat.Resolve(mission, attacker, defender, mission.PlanetSector, now);
                _db.SectorBattleReports.Add(report);
                _db.Reports.Add(new Report { UserId = mission.AttackerUserId, Title = report.Title, Body = report.Body, CreatedAtUtc = now });
                if (mission.DefenderUserId.HasValue) _db.Reports.Add(new Report { UserId = mission.DefenderUserId.Value, Title = report.Title, Body = report.Body, CreatedAtUtc = now });
                TempData["Message"] = "Kampfbericht erstellt.";
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException) { TempData["Error"] = ex.Message; }
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
            int userId = CurrentUserId();
            var user = LoadCurrentUser(userId);
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
            int userId = CurrentUserId();
            var playerBase = LoadCurrentBase(userId);
            var now = DateTime.UtcNow;
            try
            {
                var mission = _db.GateMissions.Include(m => m.MissionTeam).Include(m => m.GateAddress).ThenInclude(a => a.Planet).ThenInclude(p => p.Sectors).ThenInclude(s => s.PlayerBase).Single(m => m.Id == gateMissionId && m.UserId == userId);
                var report = _gateMissions.CompleteMission(mission, playerBase, _db.GateAddresses.ToList(), now);
                _db.GateMissionReports.Add(report);
                if (mission.MissionType == GateMissionType.FoundColony && report.Outcome != GateMissionOutcome.WoundedOrLosses)
                {
                    var user = _db.Users.Include(u => u.KnownGateAddresses).Single(u => u.Id == userId);
                    var planets = _db.Planets.Include(p => p.Sectors).ThenInclude(s => s.PlayerBase).OrderBy(p => p.Id).ToList();
                    report.Summary += " " + _gateMissions.ApplyFoundColonyResult(user, mission.GateAddress, planets, now);
                }
                if (mission.MissionType == GateMissionType.AnalyzeAddress && report.Outcome != GateMissionOutcome.WoundedOrLosses)
                {
                    var knownIds = _db.KnownGateAddresses.Where(k => k.UserId == userId).Select(k => k.GateAddressId).ToList();
                    var nextAddress = _db.GateAddresses.Where(a => a.IsNeutralPve && !knownIds.Contains(a.Id)).OrderBy(a => a.RiskLevel).FirstOrDefault();
                    if (nextAddress != null)
                    {
                        _db.KnownGateAddresses.Add(new KnownGateAddress { UserId = userId, GateAddressId = nextAddress.Id, DiscoveredAtUtc = now, DiscoveryMethod = "Adresse analysieren" });
                        report.Summary += " Neue Adresse freigeschaltet: " + nextAddress.Code + ".";
                    }
                }
                _db.Reports.Add(new Report { UserId = userId, Title = "Gate-Mission: " + mission.MissionType, Body = report.Summary, CreatedAtUtc = now });
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
            int userId = CurrentUserId();
            var user = LoadCurrentUser(userId);
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

        private int CurrentUserId()
        {
            return HttpContext.Items.TryGetValue(RequireLoginAttribute.UserIdItemKey, out var value) && value is int userId
                ? userId
                : HttpContext.Session.GetInt32("UserId")!.Value;
        }

        private IActionResult GameView(string view)
        {
            int userId = CurrentUserId();
            var user = LoadCurrentUser(userId);
            EnsureGateAccessForUser(user);
            var playerBase = LoadCurrentBase(user.Id);
            var now = DateTime.UtcNow;
            user.LastSeenAtUtc = now;
            var protectionStatus = _db.PlayerProtectionStatuses.SingleOrDefault(p => p.UserId == user.Id);
            if (protectionStatus != null)
            {
                var ownBases = _db.PlayerBases.Where(b => b.UserId == user.Id).Include(b => b.Resources).Include(b => b.Ships).Include(b => b.BuildingLevels).ToList();
                protectionStatus.Score = _ranking.CalculateUserScore(ownBases);
            }
            if (view == "Reports")
            {
                foreach (var r in _db.Reports.Where(r => r.UserId == user.Id && !r.IsRead).ToList()) r.IsRead = true;
            }
            if (view == "Mailbox")
            {
                foreach (var m in _db.PlayerMessages.Where(m => m.RecipientUserId == user.Id && m.ReadAtUtc == null).ToList()) m.ReadAtUtc = now;
            }
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
            bool queueFull = playerBase.BuildQueue.Count >= BuildQueueService.MaxQueueLength;
            var buildings = _catalog.GetAll().Select(d =>
            {
                int queuedForType = playerBase.BuildQueue.Count(q => q.BuildingType == d.Type);
                int level = playerBase.BuildingLevels.GetLevel(d.Type) + queuedForType;
                var cost = _catalog.CalculateCost(d.Type, level);
                return new BuildingUpgradeViewModel
                {
                    Type = d.Type,
                    Name = d.Name,
                    Level = playerBase.BuildingLevels.GetLevel(d.Type),
                    Cost = cost,
                    BuildSeconds = _catalog.CalculateBuildSeconds(d.Type, level, playerBase.BuildingLevels.CommandCenter),
                    CanAfford = _resources.HasEnough(playerBase.Resources, cost),
                    QueueBusy = queueFull
                };
            }).ToList();
            var playerRankings = BuildPlayerRankings();
            var allianceRankings = BuildAllianceRankings(playerRankings);
            var galaxyEntries = BuildGalaxyEntries(planet.Id, user);
            var inboxMessages = _db.PlayerMessages.Include(m => m.SenderUser).Where(m => m.RecipientUserId == user.Id && !m.IsDeletedByRecipient).OrderByDescending(m => m.CreatedAtUtc).ToList();
            var sentMessages = _db.PlayerMessages.Include(m => m.RecipientUser).Where(m => m.SenderUserId == user.Id && !m.IsDeletedBySender).OrderByDescending(m => m.CreatedAtUtc).ToList();
            var messageablePlayers = _db.Users.Where(u => !u.IsNpc && u.Id != user.Id).OrderBy(u => u.UserName).ToList();
            int unreadReportCount = _db.Reports.Count(r => r.UserId == user.Id && !r.IsRead);
            int unreadMessageCount = inboxMessages.Count(m => !m.ReadAtUtc.HasValue);
            var model = new OverviewViewModel { User = user, Base = playerBase, Planet = planet, Hourly = _economy.CalculateHourlyProduction(playerBase.BuildingLevels, user.ResearchLevels, user.Faction, sectorBonus), Sectors = planet.Sectors.OrderBy(s => s.Number).ToList(), Reports = _db.Reports.Where(r => r.UserId == user.Id).OrderByDescending(r => r.CreatedAtUtc).ToList(), Buildings = buildings, ActiveBuild = playerBase.BuildQueue.OrderBy(q => q.CompletesAtUtc).FirstOrDefault(), BuildQueue = playerBase.BuildQueue.OrderBy(q => q.CompletesAtUtc).ToList(), NowUtc = now, Researches = BuildResearchViewModels(user, playerBase), ActiveResearch = user.ResearchQueue.OrderBy(q => q.CompletesAtUtc).FirstOrDefault(), DefenseModifier = _factionModifiers.GetDefenseMultiplier(user.Faction), KnownGateAddresses = _db.KnownGateAddresses.Include(k => k.GateAddress).Where(k => k.UserId == user.Id).ToList(), MissionTeams = _db.MissionTeams.Where(t => t.UserId == user.Id).ToList(), ActiveGateMissions = _db.GateMissions.Include(m => m.GateAddress).Include(m => m.MissionTeam).Where(m => m.UserId == user.Id && !m.IsCompleted).ToList(), GateMissionReports = _db.GateMissionReports.Include(r => r.GateMission).ThenInclude(m => m.GateAddress).Where(r => r.UserId == user.Id).OrderByDescending(r => r.CreatedAtUtc).ToList(), ActiveSectorClaims = activeSectorClaims, ControlledSectors = controlledSectors, SectorBonus = sectorBonus, PlanetInfluences = BuildPlanetInfluences(planet.Id), OwnInfluence = _localSectors.CalculateInfluence(playerBase, user, controlledSectors, activeSectorClaims.Where(c => c.UserId == user.Id)), ActiveMarketOrders = _db.PlanetMarketOrders.Include(o => o.SellerUser).Where(o => o.PlanetId == planet.Id && o.CompletedAtUtc == null && o.CancelledAtUtc == null && !o.ReservedReturned && o.ExpiresAtUtc > now).OrderBy(o => o.ExpiresAtUtc).ToList(), OwnMarketOrders = _db.PlanetMarketOrders.Where(o => o.PlanetId == planet.Id && o.SellerUserId == user.Id).OrderByDescending(o => o.CreatedAtUtc).ToList(), TradeReports = _db.TradeReports.Where(r => r.UserId == user.Id).OrderByDescending(r => r.CreatedAtUtc).ToList(), ShipDefinitions = BuildShipViewModels(user, playerBase), ActiveShipBuild = playerBase.ShipyardQueue.OrderBy(q => q.CompletesAtUtc).FirstOrDefault(), FleetTargets = _db.PlayerBases.Include(b=>b.User).Include(b=>b.PlanetSector).ThenInclude(s=>s.Planet).Where(b=>b.Id!=playerBase.Id).ToList(), ActiveFleets = _db.FleetMovements.Include(f=>f.TargetBase).ThenInclude(b=>b.PlanetSector).Where(f=>f.UserId==user.Id && f.Status!=FleetMovementStatus.Completed).ToList(), FleetReports = _db.FleetReports.Where(r=>r.UserId==user.Id).OrderByDescending(r=>r.CreatedAtUtc).ToList(), OrbitPresences = BuildOrbitPresences(planet.Id), EspionageTargets = _db.PlayerBases.Include(b=>b.User).Include(b=>b.Faction).Include(b=>b.PlanetSector).ThenInclude(s=>s.Planet).Where(b=>b.Id!=playerBase.Id).ToList(), IntelligenceReports = _db.IntelligenceReports.Where(r=>r.UserId==user.Id && !r.IsWarning).OrderByDescending(r=>r.CreatedAtUtc).ToList(), SpyWarnings = _db.IntelligenceReports.Where(r=>r.UserId==user.Id && r.IsWarning).OrderByDescending(r=>r.CreatedAtUtc).ToList(), ActiveLocalCombats = _db.LocalCombatMissions.Include(m=>m.PlanetSector).Where(m=>m.PlanetSector.PlanetId==planet.Id && !m.CompletedAtUtc.HasValue).OrderBy(m=>m.ResolvesAtUtc).ToList(), SectorBattleReports = _db.SectorBattleReports.Include(r=>r.PlanetSector).Where(r=>r.UserId==user.Id).OrderByDescending(r=>r.CreatedAtUtc).ToList() , OwnAlliance = _db.AllianceMembers.Include(m=>m.Alliance).ThenInclude(a=>a.Members).ThenInclude(m=>m.User).Where(m=>m.UserId==user.Id).Select(m=>m.Alliance).FirstOrDefault(), Alliances = _db.Alliances.Include(a=>a.Members).ThenInclude(m=>m.User).OrderBy(a=>a.Tag).ToList(), AllianceApplications = _db.AllianceApplications.Include(a=>a.User).Where(a=>a.AcceptedAtUtc==null&&a.RejectedAtUtc==null).ToList(), ActiveSpaceCombats = _db.SpaceCombatMissions.Include(m=>m.TargetBase).Where(m=>m.AttackerUserId==user.Id&&!m.CompletedAtUtc.HasValue).ToList(), SpaceCombatReports = _db.SpaceCombatReports.Where(r=>r.UserId==user.Id).OrderByDescending(r=>r.CreatedAtUtc).ToList(), DebrisFields = _db.DebrisFields.Where(d=>!d.IsRecycled).OrderByDescending(d=>d.CreatedAtUtc).ToList(), ProtectionStatus = protectionStatus, PlayerRankings = playerRankings, AllianceRankings = allianceRankings, GalaxyEntries = galaxyEntries, InboxMessages = inboxMessages, SentMessages = sentMessages, MessageablePlayers = messageablePlayers, UnreadReportCount = unreadReportCount, UnreadMessageCount = unreadMessageCount };
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

        private System.Collections.Generic.IDictionary<int, string> BuildAllianceTagByUserId()
        {
            return _db.AllianceMembers.Include(m => m.Alliance).ToList().ToDictionary(m => m.UserId, m => m.Alliance.Tag);
        }

        private System.Collections.Generic.IList<PlayerRankingEntry> BuildPlayerRankings()
        {
            var bases = _db.PlayerBases.Include(b => b.User).ThenInclude(u => u.Faction).Include(b => b.Resources).Include(b => b.Ships).Include(b => b.BuildingLevels).Where(b => !b.User.IsNpc).ToList();
            return _ranking.BuildPlayerRankings(bases, BuildAllianceTagByUserId(), OnlineWindow, DateTime.UtcNow);
        }

        private System.Collections.Generic.IList<AllianceRankingEntry> BuildAllianceRankings(System.Collections.Generic.IList<PlayerRankingEntry> playerRankings)
        {
            var members = _db.AllianceMembers.Include(m => m.Alliance).ToList();
            var scoreByUserId = playerRankings.ToDictionary(p => p.UserId, p => p.Score);
            return _ranking.BuildAllianceRankings(members, scoreByUserId);
        }

        private System.Collections.Generic.IList<GalaxyEntry> BuildGalaxyEntries(int planetId, User currentUser)
        {
            var bases = _db.PlayerBases.Include(b => b.User).ThenInclude(u => u.ResearchLevels).Include(b => b.User).ThenInclude(u => u.Faction).Include(b => b.BuildingLevels).Include(b => b.PlanetSector).Where(b => b.PlanetSector.PlanetId == planetId && !b.User.IsNpc).ToList();
            var allianceTagByUserId = BuildAllianceTagByUserId();
            var sectors = _db.PlanetSectors.Include(s => s.SectorControl).Where(s => s.PlanetId == planetId && s.SectorControl != null).ToList();
            var claims = _db.SectorClaims.Include(c => c.PlanetSector).Where(c => !c.IsCompleted && c.PlanetSector.PlanetId == planetId).ToList();
            var now = DateTime.UtcNow;
            return bases.Select(b => new GalaxyEntry
            {
                PlayerBaseId = b.Id,
                BaseName = b.Name,
                UserId = b.UserId,
                UserName = b.User.UserName,
                FactionShortName = b.User.Faction?.ShortName,
                AllianceTag = allianceTagByUserId.TryGetValue(b.UserId, out var tag) ? tag : null,
                SectorNumber = b.PlanetSector.Number,
                SectorName = b.PlanetSector.Name,
                InfluenceScore = _localSectors.CalculateInfluence(b, b.User, sectors.Where(s => s.SectorControl.UserId == b.UserId), claims.Where(c => c.UserId == b.UserId)),
                IsOnline = b.User.LastSeenAtUtc.HasValue && b.User.LastSeenAtUtc.Value >= now - OnlineWindow,
                IsOwnBase = b.UserId == currentUser.Id
            }).OrderByDescending(g => g.InfluenceScore).ToList();
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
            var user = _db.Users.Include(u => u.Faction).Include(u => u.ResearchLevels).Include(u => u.ResearchQueue).Include(u => u.KnownGateAddresses).First(u => u.Id == userId);
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
