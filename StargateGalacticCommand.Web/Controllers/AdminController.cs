using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StargateGalacticCommand.Data;
using StargateGalacticCommand.Web.Filters;
using StargateGalacticCommand.Web.Models;

namespace StargateGalacticCommand.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly GameDbContext _db;
        private readonly GameServerService _servers;
        private readonly IConfiguration _configuration;

        public AdminController(GameDbContext db, GameServerService servers, IConfiguration configuration)
        {
            _db = db;
            _servers = servers;
            _configuration = configuration;
        }

        public IActionResult Login() { return View(new AdminLoginViewModel()); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(AdminLoginViewModel model)
        {
            var expected = _configuration["Admin:Password"];
            if (string.IsNullOrEmpty(expected) || model.Password != expected)
            {
                model.Error = "Passwort ist falsch.";
                return View(model);
            }
            HttpContext.Session.SetString("IsAdmin", "true");
            return RedirectToAction("Servers");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("IsAdmin");
            return RedirectToAction("Login");
        }

        [RequireAdmin]
        public IActionResult Servers()
        {
            var model = new AdminServerListViewModel
            {
                Servers = _servers.ListServers().Select(s => new AdminServerListEntry
                {
                    Server = s,
                    PlayerCount = _db.Users.Count(u => u.ServerId == s.Id && !u.IsNpc)
                }).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAdmin]
        public IActionResult Create(string name, string description)
        {
            try { _servers.CreateServer(name, description); TempData["Message"] = "Server erstellt."; }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException) { TempData["Error"] = ex.Message; }
            return RedirectToAction("Servers");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAdmin]
        public IActionResult Start(int id)
        {
            try { _servers.StartServer(id); TempData["Message"] = "Server gestartet."; }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction("Servers");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAdmin]
        public IActionResult Pause(int id)
        {
            try { _servers.PauseServer(id); TempData["Message"] = "Server pausiert."; }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction("Servers");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAdmin]
        public IActionResult Stop(int id)
        {
            try { _servers.StopServer(id); TempData["Message"] = "Server gestoppt."; }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction("Servers");
        }
    }
}
