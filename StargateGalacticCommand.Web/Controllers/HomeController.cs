using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StargateGalacticCommand.Data;
using StargateGalacticCommand.Web.Models;

namespace StargateGalacticCommand.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly GameDbContext _dbContext;

        public HomeController(GameDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            HomeIndexViewModel model = new HomeIndexViewModel
            {
                OpenPlanets = _dbContext.Planets
                    .Where(p => !p.IsCanonicalRestricted)
                    .OrderBy(p => p.Name)
                    .ToList(),
                BaseSectors = _dbContext.BaseSectors
                    .Include(b => b.Planet)
                    .Include(b => b.Resources)
                    .Include(b => b.Buildings)
                    .OrderBy(b => b.Planet.Name)
                    .ThenBy(b => b.Name)
                    .ToList(),
                RestrictedWorldCount = _dbContext.Planets.Count(p => p.IsCanonicalRestricted)
            };

            return View(model);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
