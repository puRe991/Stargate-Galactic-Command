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
    public class AccountController : Controller
    {
        private readonly GameDbContext _db; private readonly RegistrationService _registration; private readonly PasswordService _passwords;
        public AccountController(GameDbContext db, RegistrationService registration, PasswordService passwords) { _db = db; _registration = registration; _passwords = passwords; }
        public IActionResult Register() { return View(new RegisterViewModel { Factions = _db.Factions.OrderBy(f => f.Id).ToList() }); }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            model.Factions = _db.Factions.OrderBy(f => f.Id).ToList();
            if (!ModelState.IsValid) return View(model);
            try
            {
                var planets = _db.Planets.Include(p => p.Sectors).ThenInclude(s => s.PlayerBase);
                var user = _registration.CreateUserWithStartBase(_db.Users, _db.Factions, planets, model.UserName, model.Email, model.Password, model.FactionId);
                _db.Users.Add(user); _db.SaveChanges(); HttpContext.Session.SetInt32("UserId", user.Id); return RedirectToAction("Overview", "Game");
            }
            catch (Exception ex) { model.Error = ex.Message; return View(model); }
        }
        public IActionResult Login() { return View(new LoginViewModel()); }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var key = (model.UserNameOrEmail ?? string.Empty).Trim().ToLowerInvariant();
            var user = _db.Users.FirstOrDefault(u => u.UserName.ToLower() == key || u.Email == key);
            if (user == null || !_passwords.Verify(model.Password, user.PasswordHash, user.PasswordSalt)) { model.Error = "Login fehlgeschlagen."; return View(model); }
            HttpContext.Session.SetInt32("UserId", user.Id); return RedirectToAction("Overview", "Game");
        }
        public IActionResult Logout() { HttpContext.Session.Clear(); return RedirectToAction("Index", "Home"); }
    }
}
