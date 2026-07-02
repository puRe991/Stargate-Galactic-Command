using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StargateGalacticCommand.Core.Services;
using StargateGalacticCommand.Data;
using StargateGalacticCommand.Web.Models;

namespace StargateGalacticCommand.Web.Controllers
{
    public class GameController : Controller
    {
        private readonly GameDbContext _db; private readonly EconomyService _economy;
        public GameController(GameDbContext db, EconomyService economy) { _db = db; _economy = economy; }
        public IActionResult Overview() { return GameView("Overview"); }
        public IActionResult Base() { return GameView("Base"); }
        public IActionResult Planet() { return GameView("Planet"); }
        public IActionResult Sectors() { return GameView("Sectors"); }
        public IActionResult Resources() { return GameView("Resources"); }
        public IActionResult Reports() { return GameView("Reports"); }
        private IActionResult GameView(string view)
        {
            int? userId = HttpContext.Session.GetInt32("UserId"); if (!userId.HasValue) return RedirectToAction("Login", "Account");
            var user = _db.Users.Include(u => u.Faction).First(u => u.Id == userId.Value);
            var playerBase = _db.PlayerBases.Include(b => b.Faction).Include(b => b.Resources).Include(b => b.BuildingLevels).Include(b => b.PlanetSector).ThenInclude(s => s.Planet).First(b => b.UserId == user.Id);
            _economy.ApplyOfflineProduction(playerBase, DateTime.UtcNow); _db.SaveChanges();
            var planet = _db.Planets.Include(p => p.Sectors).ThenInclude(s => s.PlayerBase).Single(p => p.Id == playerBase.PlanetSector.PlanetId);
            var model = new OverviewViewModel { User = user, Base = playerBase, Planet = planet, Hourly = _economy.CalculateHourlyProduction(playerBase.BuildingLevels), Sectors = planet.Sectors.OrderBy(s => s.Number).ToList(), Reports = _db.Reports.Where(r => r.UserId == user.Id).OrderByDescending(r => r.CreatedAtUtc).ToList() };
            return View(view, model);
        }
    }
}
