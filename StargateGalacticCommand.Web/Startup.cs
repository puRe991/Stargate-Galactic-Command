using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StargateGalacticCommand.Core.Services;
using StargateGalacticCommand.Data;
using StargateGalacticCommand.Web.Hubs;

namespace StargateGalacticCommand.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddSession();
            services.AddSignalR();
            services.AddDbContext<GameDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<EconomyService>();
            services.AddScoped<BuildingCatalogService>();
            services.AddScoped<ResourceService>();
            services.AddScoped<PlanetMarketService>();
            services.AddScoped<BuildQueueService>();
            services.AddScoped<ShipyardService>();
            services.AddScoped<FleetService>();
            services.AddScoped<EspionageService>();
            services.AddScoped<ResearchCatalogService>();
            services.AddScoped<ResearchQueueService>();
            services.AddScoped<FactionModifierService>();
            services.AddScoped<SeasonService>();
            services.AddScoped<SkillTreeService>();
            services.AddScoped<GateMissionService>();
            services.AddScoped<LocalSectorService>();
            services.AddScoped<LocalCombatService>();
            services.AddScoped<AllianceService>();
            services.AddScoped<SpaceCombatService>();
            services.AddScoped<PasswordService>();
            services.AddScoped<RegistrationService>();
            services.AddScoped<LoginSecurityService>();
            services.AddScoped<RankingService>();
            services.AddScoped<MessageService>();
            services.AddScoped<ContractService>();
            services.AddScoped<AchievementService>();
            services.AddScoped<AllianceWarService>();
            services.AddScoped<AscensionService>();
            services.AddScoped<WorldEventService>();
            services.AddScoped<TradeRouteService>();
            services.AddScoped<MentorService>();
            services.AddScoped<DiplomacyService>();
            services.AddScoped<QuestlineService>();
            services.AddScoped<GameServerService>();
            services.AddScoped<SpecialResourceCatalogService>();
            services.AddScoped<SpecialResourceService>();
            services.AddScoped<ChatService>();
        }

        public const string DefaultAdminPassword = "change-me";

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, GameDbContext dbContext, GateMissionService gateMissionService, ILogger<Startup> logger)
        {
            CheckAdminPassword(env, logger);
            DatabaseInitializer.Initialize(dbContext, gateMissionService);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapHub<ChatHub>("/hubs/chat");
            });
        }

        // The default Admin:Password from appsettings.json ("change-me") grants full
        // admin access (create/pause/stop game servers) to anyone who reads the repo.
        // Blocking startup outside Development turns a silently-forgotten override into
        // an immediate, loud failure instead of an exploitable production deployment.
        private void CheckAdminPassword(IWebHostEnvironment env, ILogger logger)
        {
            var password = Configuration["Admin:Password"];
            if (password != DefaultAdminPassword) return;

            if (env.IsDevelopment())
            {
                logger.LogWarning("Admin:Password ist noch auf den Standardwert '{Default}' gesetzt. Vor jedem produktiven Deployment per Umgebungsvariable Admin__Password überschreiben.", DefaultAdminPassword);
                return;
            }

            throw new InvalidOperationException(
                $"Admin:Password steht noch auf dem Standardwert '{DefaultAdminPassword}'. " +
                "Setze die Umgebungsvariable Admin__Password auf ein sicheres Passwort, bevor die Anwendung außerhalb von Development gestartet wird.");
        }
    }
}
