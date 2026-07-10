using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Data;
using StargateGalacticCommand.Web.Models;

namespace StargateGalacticCommand.Web.Controllers
{
    public class ServerController : Controller
    {
        private readonly GameDbContext _db;
        public ServerController(GameDbContext db) { _db = db; }

        public IActionResult Select()
        {
            var servers = _db.GameServers.Where(s => s.Status != ServerStatus.Stopped).OrderBy(s => s.Id).ToList();
            var model = new ServerListViewModel
            {
                Servers = servers.Select(s => new ServerListEntry
                {
                    Server = s,
                    PlayerCount = _db.Users.Count(u => u.ServerId == s.Id && !u.IsNpc)
                }).ToList()
            };
            return View(model);
        }

        public IActionResult Choose(int id)
        {
            var server = _db.GameServers.SingleOrDefault(s => s.Id == id && s.Status != ServerStatus.Stopped);
            if (server == null) return RedirectToAction("Select");
            HttpContext.Session.SetInt32("ServerId", server.Id);
            return RedirectToAction("Login", "Account");
        }
    }
}
