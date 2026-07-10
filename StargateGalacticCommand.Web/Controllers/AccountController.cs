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
    public class AccountController : Controller
    {
        private readonly GameDbContext _db; private readonly RegistrationService _registration; private readonly PasswordService _passwords; private readonly LoginSecurityService _loginSecurity;
        public AccountController(GameDbContext db, RegistrationService registration, PasswordService passwords, LoginSecurityService loginSecurity) { _db = db; _registration = registration; _passwords = passwords; _loginSecurity = loginSecurity; }

        private GameServer LoadSelectedServer()
        {
            var serverId = HttpContext.Session.GetInt32("ServerId");
            if (!serverId.HasValue) return null;
            return _db.GameServers.SingleOrDefault(s => s.Id == serverId.Value && s.Status != ServerStatus.Stopped);
        }

        public IActionResult Register()
        {
            var server = LoadSelectedServer();
            if (server == null) return RedirectToAction("Select", "Server");
            var model = new RegisterViewModel { Factions = _db.Factions.OrderBy(f => f.Id).ToList(), ServerName = server.Name };
            if (server.Status == ServerStatus.Paused) model.Error = "Dieser Server nimmt aktuell keine neuen Spieler auf.";
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            var server = LoadSelectedServer();
            if (server == null) return RedirectToAction("Select", "Server");
            model.Factions = _db.Factions.OrderBy(f => f.Id).ToList();
            model.ServerName = server.Name;
            if (server.Status == ServerStatus.Paused) { model.Error = "Dieser Server nimmt aktuell keine neuen Spieler auf."; return View(model); }
            if (!ModelState.IsValid) return View(model);
            try
            {
                var planets = _db.Planets.Include(p => p.Sectors).ThenInclude(s => s.PlayerBase);
                var user = _registration.CreateUserWithStartBase(_db.Users, _db.Factions, planets, server.Id, model.UserName, model.Email, model.Password, model.FactionId);
                _db.Users.Add(user);
                _db.PlayerProtectionStatuses.Add(new PlayerProtectionStatus { User = user, ProtectedUntilUtc = user.CreatedAtUtc.AddDays(3), Score = 0 });
                _db.SaveChanges(); HttpContext.Session.SetInt32("UserId", user.Id); return RedirectToAction("Overview", "Game");
            }
            catch (Exception ex) { model.Error = ex.Message; return View(model); }
        }
        public IActionResult Login()
        {
            var server = LoadSelectedServer();
            if (server == null) return RedirectToAction("Select", "Server");
            return View(new LoginViewModel { ServerName = server.Name });
        }
        public static User? FindLoginCandidate(IQueryable<User> users, int serverId, string? userNameOrEmail)
        {
            var key = (userNameOrEmail ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(key)) return null;
            return users.FirstOrDefault(u => u.ServerId == serverId && !u.IsNpc && (u.UserName.ToLower() == key || u.Email == key));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            var server = LoadSelectedServer();
            if (server == null) return RedirectToAction("Select", "Server");
            model.ServerName = server.Name;
            if (!ModelState.IsValid) return View(model);

            var now = DateTime.UtcNow;
            var ipHash = LoginSecurityService.HashIp(HttpContext.Connection.RemoteIpAddress?.ToString());
            var usernameKey = LoginSecurityService.NormalizeUsernameKey(model.UserNameOrEmail);
            var lockout = _loginSecurity.CheckLockout(_db.LoginAttempts, server.Id, ipHash, usernameKey, now);
            if (lockout.IsLockedOut)
            {
                var waitMinutes = Math.Max(1, (int)Math.Ceiling((lockout.RetryAfterUtc.Value - now).TotalMinutes));
                model.Error = "Zu viele Fehlversuche. Bitte in " + waitMinutes + " Minute(n) erneut versuchen.";
                return View(model);
            }

            var user = FindLoginCandidate(_db.Users, server.Id, model.UserNameOrEmail);
            bool success = user != null && _passwords.Verify(model.Password, user.PasswordHash, user.PasswordSalt);
            _db.LoginAttempts.Add(_loginSecurity.RecordAttempt(server.Id, ipHash, usernameKey, success, now));
            _db.LoginAttempts.RemoveRange(_db.LoginAttempts.Where(a => a.AttemptedAtUtc < now.AddDays(-LoginSecurityService.RetentionDays)));
            if (!success) { model.Error = "Login fehlgeschlagen."; _db.SaveChanges(); return View(model); }
            user.LastSeenAtUtc = now;
            _db.SaveChanges();
            HttpContext.Session.SetInt32("UserId", user.Id); return RedirectToAction("Overview", "Game");
        }
        public IActionResult Logout() { HttpContext.Session.Clear(); return RedirectToAction("Index", "Home"); }
    }
}
