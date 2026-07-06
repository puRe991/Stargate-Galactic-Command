using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StargateGalacticCommand.Data;
using StargateGalacticCommand.Web.Filters;

namespace StargateGalacticCommand.Web.Controllers
{
    [RequireLogin]
    public class ExplorationController : Controller
    {
        private readonly GameDbContext _db;

        public ExplorationController(GameDbContext db)
        {
            _db = db;
        }

        // Technischer Spike fuer Phase 1 der MMO-Roadmap (siehe ROADMAP.md):
        // Beweist die Machbarkeit eines Echtzeit-2D-Clients ueber SignalR neben
        // der bestehenden Razor-Anwendung. Enthaelt noch keine Spiellogik.
        public IActionResult Spike()
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var commanderName = _db.Users.Where(u => u.Id == userId).Select(u => u.UserName).FirstOrDefault() ?? "Commander";
            ViewData["CommanderName"] = commanderName;
            return View();
        }
    }
}
