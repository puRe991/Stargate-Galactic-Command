using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StargateGalacticCommand.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Factions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    ShortName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Factions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameServers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GalaxySeed = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameServers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoginAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    IpHash = table.Column<string>(type: "TEXT", nullable: false),
                    UsernameKey = table.Column<string>(type: "TEXT", nullable: false),
                    Succeeded = table.Column<bool>(type: "INTEGER", nullable: false),
                    AttemptedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginAttempts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TradeTaxRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BaseFeeRate = table.Column<double>(type: "REAL", nullable: false),
                    LucianAllianceReduction = table.Column<double>(type: "REAL", nullable: false),
                    TradingPostReduction = table.Column<double>(type: "REAL", nullable: false),
                    MaxIntelAmount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeTaxRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Planets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Galaxy = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    StargateActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Planets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Planets_GameServers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "GameServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordSalt = table.Column<string>(type: "TEXT", nullable: false),
                    FactionId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsNpc = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    LastSeenAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AscensionCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LastAscendedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Factions_FactionId",
                        column: x => x.FactionId,
                        principalTable: "Factions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Users_GameServers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "GameServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorldEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndsAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GoalProgress = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentProgress = table.Column<int>(type: "INTEGER", nullable: false),
                    ResolvedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorldEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorldEvents_GameServers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "GameServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GateAddresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlanetId = table.Column<int>(type: "INTEGER", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    WorldName = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    IsNeutralPve = table.Column<bool>(type: "INTEGER", nullable: false),
                    RiskLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    AnomalyFound = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GateAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GateAddresses_GameServers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "GameServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GateAddresses_Planets_PlanetId",
                        column: x => x.PlanetId,
                        principalTable: "Planets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PlanetSectors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlanetId = table.Column<int>(type: "INTEGER", nullable: false),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IsSettlementSector = table.Column<bool>(type: "INTEGER", nullable: false),
                    SectorType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanetSectors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanetSectors_Planets_PlanetId",
                        column: x => x.PlanetId,
                        principalTable: "Planets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AchievementProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    AchievementKey = table.Column<string>(type: "TEXT", nullable: false),
                    UnlockedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AchievementProgresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alliances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Tag = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    FounderUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alliances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alliances_GameServers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "GameServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Alliances_Users_FounderUserId",
                        column: x => x.FounderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CharacterSkills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    MilitaryLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    ScienceLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    DiplomacyLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    UnspentPoints = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterSkills_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    ContractKey = table.Column<string>(type: "TEXT", nullable: false),
                    PeriodStartUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ClaimedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractProgresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MissionTeams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Strength = table.Column<int>(type: "INTEGER", nullable: false),
                    Science = table.Column<int>(type: "INTEGER", nullable: false),
                    Diplomacy = table.Column<int>(type: "INTEGER", nullable: false),
                    Stealth = table.Column<int>(type: "INTEGER", nullable: false),
                    CarryCapacity = table.Column<int>(type: "INTEGER", nullable: false),
                    Risk = table.Column<int>(type: "INTEGER", nullable: false),
                    IsAvailable = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MissionTeams_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SenderUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    RecipientUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Body = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReadAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeletedBySender = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDeletedByRecipient = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerMessages_Users_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerMessages_Users_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerProtectionStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProtectedUntilUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    LastAttackedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerProtectionStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerProtectionStatuses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestlineStepProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    StepKey = table.Column<string>(type: "TEXT", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestlineStepProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestlineStepProgresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Body = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reports_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResearchLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    GateAddressing = table.Column<int>(type: "INTEGER", nullable: false),
                    NaquadahEnergyTechnology = table.Column<int>(type: "INTEGER", nullable: false),
                    ShieldTechnology = table.Column<int>(type: "INTEGER", nullable: false),
                    Hyperdrive = table.Column<int>(type: "INTEGER", nullable: false),
                    Sensorics = table.Column<int>(type: "INTEGER", nullable: false),
                    Medicine = table.Column<int>(type: "INTEGER", nullable: false),
                    StealthTechnology = table.Column<int>(type: "INTEGER", nullable: false),
                    Diplomacy = table.Column<int>(type: "INTEGER", nullable: false),
                    Logistics = table.Column<int>(type: "INTEGER", nullable: false),
                    AdvancedNaquadahRefining = table.Column<int>(type: "INTEGER", nullable: false),
                    AutomatedTriniumExtraction = table.Column<int>(type: "INTEGER", nullable: false),
                    StructuralEngineering = table.Column<int>(type: "INTEGER", nullable: false),
                    AdvancedShipEngineering = table.Column<int>(type: "INTEGER", nullable: false),
                    XenoArchaeology = table.Column<int>(type: "INTEGER", nullable: false),
                    AsgardDataAnalysis = table.Column<int>(type: "INTEGER", nullable: false),
                    Bc304Tactics = table.Column<int>(type: "INTEGER", nullable: false),
                    IrisSecurityProtocols = table.Column<int>(type: "INTEGER", nullable: false),
                    NaquadahReactorMiniaturization = table.Column<int>(type: "INTEGER", nullable: false),
                    AncientOutpostTechnology = table.Column<int>(type: "INTEGER", nullable: false),
                    PrometheusEngineering = table.Column<int>(type: "INTEGER", nullable: false),
                    StargateNetworkMapping = table.Column<int>(type: "INTEGER", nullable: false),
                    AsgardBeamingTechnology = table.Column<int>(type: "INTEGER", nullable: false),
                    ZeroPointModuleTheory = table.Column<int>(type: "INTEGER", nullable: false),
                    StaffWeaponDiscipline = table.Column<int>(type: "INTEGER", nullable: false),
                    HatakCommandStructure = table.Column<int>(type: "INTEGER", nullable: false),
                    JaffaWarriorCode = table.Column<int>(type: "INTEGER", nullable: false),
                    SymbioteEfficiency = table.Column<int>(type: "INTEGER", nullable: false),
                    GroundAssaultTactics = table.Column<int>(type: "INTEGER", nullable: false),
                    FortifiedGarrisons = table.Column<int>(type: "INTEGER", nullable: false),
                    KelNoReemTraining = table.Column<int>(type: "INTEGER", nullable: false),
                    HonorGuardProtocols = table.Column<int>(type: "INTEGER", nullable: false),
                    FreeJaffaNationLogistics = table.Column<int>(type: "INTEGER", nullable: false),
                    CovertInfiltration = table.Column<int>(type: "INTEGER", nullable: false),
                    GoauldSabotage = table.Column<int>(type: "INTEGER", nullable: false),
                    CloakFieldCoordination = table.Column<int>(type: "INTEGER", nullable: false),
                    SymbioteHealing = table.Column<int>(type: "INTEGER", nullable: false),
                    DeepCoverNetworks = table.Column<int>(type: "INTEGER", nullable: false),
                    HostBondingTechnology = table.Column<int>(type: "INTEGER", nullable: false),
                    IntelligenceNetworkExpansion = table.Column<int>(type: "INTEGER", nullable: false),
                    SystemLordDossiers = table.Column<int>(type: "INTEGER", nullable: false),
                    ShadowCouncilInfluence = table.Column<int>(type: "INTEGER", nullable: false),
                    SmugglingRoutes = table.Column<int>(type: "INTEGER", nullable: false),
                    BlackMarketLogistics = table.Column<int>(type: "INTEGER", nullable: false),
                    MercenaryContracts = table.Column<int>(type: "INTEGER", nullable: false),
                    PirateNetworkConnections = table.Column<int>(type: "INTEGER", nullable: false),
                    RuthlessNegotiation = table.Column<int>(type: "INTEGER", nullable: false),
                    StolenTechnologyIntegration = table.Column<int>(type: "INTEGER", nullable: false),
                    HiddenCaches = table.Column<int>(type: "INTEGER", nullable: false),
                    ExtortionNetworks = table.Column<int>(type: "INTEGER", nullable: false),
                    WarlordAmbitions = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResearchLevels_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResearchQueueItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    ResearchType = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletesAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchQueueItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResearchQueueItems_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorldEventContributions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorldEventId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    LastContributedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RewardGrantedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorldEventContributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorldEventContributions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorldEventContributions_WorldEvents_WorldEventId",
                        column: x => x.WorldEventId,
                        principalTable: "WorldEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KnownGateAddresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    GateAddressId = table.Column<int>(type: "INTEGER", nullable: false),
                    DiscoveredAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DiscoveryMethod = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnownGateAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KnownGateAddresses_GateAddresses_GateAddressId",
                        column: x => x.GateAddressId,
                        principalTable: "GateAddresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KnownGateAddresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocalActionReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlanetSectorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Body = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalActionReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalActionReports_PlanetSectors_PlanetSectorId",
                        column: x => x.PlanetSectorId,
                        principalTable: "PlanetSectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalActionReports_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocalCombatMissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AttackerUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    DefenderUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    PlanetSectorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Objective = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ResolvesAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AttackerWon = table.Column<bool>(type: "INTEGER", nullable: false),
                    AttackerLosses = table.Column<int>(type: "INTEGER", nullable: false),
                    DefenderLosses = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalCombatMissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalCombatMissions_PlanetSectors_PlanetSectorId",
                        column: x => x.PlanetSectorId,
                        principalTable: "PlanetSectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LocalCombatMissions_Users_AttackerUserId",
                        column: x => x.AttackerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LocalCombatMissions_Users_DefenderUserId",
                        column: x => x.DefenderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerBases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    FactionId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlanetSectorId = table.Column<int>(type: "INTEGER", nullable: false),
                    LastResourceUpdateUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerBases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerBases_Factions_FactionId",
                        column: x => x.FactionId,
                        principalTable: "Factions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerBases_PlanetSectors_PlanetSectorId",
                        column: x => x.PlanetSectorId,
                        principalTable: "PlanetSectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerBases_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SectorClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlanetSectorId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletesAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectorClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SectorClaims_PlanetSectors_PlanetSectorId",
                        column: x => x.PlanetSectorId,
                        principalTable: "PlanetSectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SectorClaims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SectorControls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlanetSectorId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    ControlledAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastReinforcedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectorControls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SectorControls_PlanetSectors_PlanetSectorId",
                        column: x => x.PlanetSectorId,
                        principalTable: "PlanetSectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SectorControls_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AllianceApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AllianceId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AcceptedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DecidedByUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllianceApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AllianceApplications_Alliances_AllianceId",
                        column: x => x.AllianceId,
                        principalTable: "Alliances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AllianceApplications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AllianceDiplomacyStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AllianceAId = table.Column<int>(type: "INTEGER", nullable: false),
                    AllianceBId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ProposedByAllianceId = table.Column<int>(type: "INTEGER", nullable: false),
                    SinceUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BrokenByAllianceId = table.Column<int>(type: "INTEGER", nullable: true),
                    LastBrokenAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllianceDiplomacyStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AllianceDiplomacyStatuses_Alliances_AllianceAId",
                        column: x => x.AllianceAId,
                        principalTable: "Alliances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AllianceDiplomacyStatuses_Alliances_AllianceBId",
                        column: x => x.AllianceBId,
                        principalTable: "Alliances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AllianceMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AllianceId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Rank = table.Column<int>(type: "INTEGER", nullable: false),
                    JoinedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MentorUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    MentorMissionRewardGrantedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MentorSectorRewardGrantedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllianceMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AllianceMembers_Alliances_AllianceId",
                        column: x => x.AllianceId,
                        principalTable: "Alliances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AllianceMembers_Users_MentorUserId",
                        column: x => x.MentorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AllianceMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AllianceWarGoals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AllianceId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlanetId = table.Column<int>(type: "INTEGER", nullable: false),
                    RequiredSectors = table.Column<int>(type: "INTEGER", nullable: false),
                    RequiredHours = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HoldStreakStartedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    AchievedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllianceWarGoals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AllianceWarGoals_Alliances_AllianceId",
                        column: x => x.AllianceId,
                        principalTable: "Alliances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AllianceWarGoals_Planets_PlanetId",
                        column: x => x.PlanetId,
                        principalTable: "Planets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GateMissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    GateAddressId = table.Column<int>(type: "INTEGER", nullable: false),
                    MissionTeamId = table.Column<int>(type: "INTEGER", nullable: false),
                    MissionType = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletesAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GateMissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GateMissions_GateAddresses_GateAddressId",
                        column: x => x.GateAddressId,
                        principalTable: "GateAddresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GateMissions_MissionTeams_MissionTeamId",
                        column: x => x.MissionTeamId,
                        principalTable: "MissionTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GateMissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DefenseUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LocalCombatMissionId = table.Column<int>(type: "INTEGER", nullable: false),
                    LocalCombatMissionId1 = table.Column<int>(type: "INTEGER", nullable: false),
                    BaseGuards = table.Column<int>(type: "INTEGER", nullable: false),
                    DefenseRings = table.Column<int>(type: "INTEGER", nullable: false),
                    SensorAlarms = table.Column<int>(type: "INTEGER", nullable: false),
                    LocalMilitia = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefenseUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DefenseUnits_LocalCombatMissions_LocalCombatMissionId",
                        column: x => x.LocalCombatMissionId,
                        principalTable: "LocalCombatMissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DefenseUnits_LocalCombatMissions_LocalCombatMissionId1",
                        column: x => x.LocalCombatMissionId1,
                        principalTable: "LocalCombatMissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroundUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LocalCombatMissionId = table.Column<int>(type: "INTEGER", nullable: false),
                    LocalCombatMissionId1 = table.Column<int>(type: "INTEGER", nullable: false),
                    SgSecurityTeams = table.Column<int>(type: "INTEGER", nullable: false),
                    Marines = table.Column<int>(type: "INTEGER", nullable: false),
                    JaffaWarriors = table.Column<int>(type: "INTEGER", nullable: false),
                    EliteJaffa = table.Column<int>(type: "INTEGER", nullable: false),
                    AgentCells = table.Column<int>(type: "INTEGER", nullable: false),
                    Saboteurs = table.Column<int>(type: "INTEGER", nullable: false),
                    Mercenaries = table.Column<int>(type: "INTEGER", nullable: false),
                    SmugglerSquads = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroundUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroundUnits_LocalCombatMissions_LocalCombatMissionId",
                        column: x => x.LocalCombatMissionId,
                        principalTable: "LocalCombatMissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroundUnits_LocalCombatMissions_LocalCombatMissionId1",
                        column: x => x.LocalCombatMissionId1,
                        principalTable: "LocalCombatMissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocalCombatRounds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LocalCombatMissionId = table.Column<int>(type: "INTEGER", nullable: false),
                    RoundNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    AttackerPower = table.Column<int>(type: "INTEGER", nullable: false),
                    DefenderPower = table.Column<int>(type: "INTEGER", nullable: false),
                    AttackerLosses = table.Column<int>(type: "INTEGER", nullable: false),
                    DefenderLosses = table.Column<int>(type: "INTEGER", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalCombatRounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalCombatRounds_LocalCombatMissions_LocalCombatMissionId",
                        column: x => x.LocalCombatMissionId,
                        principalTable: "LocalCombatMissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SectorBattleReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    LocalCombatMissionId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlanetSectorId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Body = table.Column<string>(type: "TEXT", nullable: false),
                    AttackerWon = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectorBattleReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SectorBattleReports_LocalCombatMissions_LocalCombatMissionId",
                        column: x => x.LocalCombatMissionId,
                        principalTable: "LocalCombatMissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SectorBattleReports_PlanetSectors_PlanetSectorId",
                        column: x => x.PlanetSectorId,
                        principalTable: "PlanetSectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SectorBattleReports_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BaseShips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerBaseId = table.Column<int>(type: "INTEGER", nullable: false),
                    F302 = table.Column<int>(type: "INTEGER", nullable: false),
                    SmallTransporter = table.Column<int>(type: "INTEGER", nullable: false),
                    SupplyShuttle = table.Column<int>(type: "INTEGER", nullable: false),
                    Teltak = table.Column<int>(type: "INTEGER", nullable: false),
                    AlkeshLightBomber = table.Column<int>(type: "INTEGER", nullable: false),
                    JaffaTransporter = table.Column<int>(type: "INTEGER", nullable: false),
                    CloakedTeltak = table.Column<int>(type: "INTEGER", nullable: false),
                    AgentTransporter = table.Column<int>(type: "INTEGER", nullable: false),
                    SmugglerTransporter = table.Column<int>(type: "INTEGER", nullable: false),
                    PirateFighter = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseShips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaseShips_PlayerBases_PlayerBaseId",
                        column: x => x.PlayerBaseId,
                        principalTable: "PlayerBases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuildingLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerBaseId = table.Column<int>(type: "INTEGER", nullable: false),
                    CommandCenter = table.Column<int>(type: "INTEGER", nullable: false),
                    NaquadahRefinery = table.Column<int>(type: "INTEGER", nullable: false),
                    TriniumMine = table.Column<int>(type: "INTEGER", nullable: false),
                    SupplyDepot = table.Column<int>(type: "INTEGER", nullable: false),
                    EnergyGenerator = table.Column<int>(type: "INTEGER", nullable: false),
                    ResearchLab = table.Column<int>(type: "INTEGER", nullable: false),
                    GateControlRoom = table.Column<int>(type: "INTEGER", nullable: false),
                    SensorStation = table.Column<int>(type: "INTEGER", nullable: false),
                    DefenseRing = table.Column<int>(type: "INTEGER", nullable: false),
                    HangarLandingZone = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildingLevels_PlayerBases_PlayerBaseId",
                        column: x => x.PlayerBaseId,
                        principalTable: "PlayerBases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuildQueueItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerBaseId = table.Column<int>(type: "INTEGER", nullable: false),
                    BuildingType = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletesAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildQueueItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildQueueItems_PlayerBases_PlayerBaseId",
                        column: x => x.PlayerBaseId,
                        principalTable: "PlayerBases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DebrisFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerBaseId = table.Column<int>(type: "INTEGER", nullable: false),
                    Naquadah = table.Column<int>(type: "INTEGER", nullable: false),
                    Trinium = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsRecycled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DebrisFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DebrisFields_PlayerBases_PlayerBaseId",
                        column: x => x.PlayerBaseId,
                        principalTable: "PlayerBases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DecoyProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerBaseId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Charges = table.Column<int>(type: "INTEGER", nullable: false),
                    FakeNaquadah = table.Column<int>(type: "INTEGER", nullable: false),
                    FakeTrinium = table.Column<int>(type: "INTEGER", nullable: false),
                    FakeSupplies = table.Column<int>(type: "INTEGER", nullable: false),
                    FakeEnergy = table.Column<int>(type: "INTEGER", nullable: false),
                    FakePersonnel = table.Column<int>(type: "INTEGER", nullable: false),
                    FakeIntel = table.Column<int>(type: "INTEGER", nullable: false),
                    FakeShipTotal = table.Column<int>(type: "INTEGER", nullable: false),
                    LastArmedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecoyProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DecoyProfiles_PlayerBases_PlayerBaseId",
                        column: x => x.PlayerBaseId,
                        principalTable: "PlayerBases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EspionageMissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    SourceBaseId = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetBaseId = table.Column<int>(type: "INTEGER", nullable: false),
                    MissionType = table.Column<int>(type: "INTEGER", nullable: false),
                    IntelSpent = table.Column<int>(type: "INTEGER", nullable: false),
                    ReportDepth = table.Column<int>(type: "INTEGER", nullable: false),
                    DetectionRiskPercent = table.Column<int>(type: "INTEGER", nullable: false),
                    WasDetected = table.Column<bool>(type: "INTEGER", nullable: false),
                    TargetCounterIntelligenceLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EspionageMissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EspionageMissions_PlayerBases_SourceBaseId",
                        column: x => x.SourceBaseId,
                        principalTable: "PlayerBases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EspionageMissions_PlayerBases_TargetBaseId",
                        column: x => x.TargetBaseId,
                        principalTable: "PlayerBases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EspionageMissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanetMarketOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlanetId = table.Column<int>(type: "INTEGER", nullable: false),
                    SellerUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    SellerBaseId = table.Column<int>(type: "INTEGER", nullable: false),
                    OfferedResource = table.Column<int>(type: "INTEGER", nullable: false),
                    OfferedAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    RequestedResource = table.Column<int>(type: "INTEGER", nullable: false),
                    RequestedAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CancelledAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReservedReturned = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanetMarketOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanetMarketOrders_Planets_PlanetId",
                        column: x => x.PlanetId,
                        principalTable: "Planets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanetMarketOrders_PlayerBases_SellerBaseId",
                        column: x => x.SellerBaseId,
                        principalTable: "PlayerBases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlanetMarketOrders_Users_SellerUserId",
                        column: x => x.SellerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourceStocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerBaseId = table.Column<int>(type: "INTEGER", nullable: false),
                    Naquadah = table.Column<int>(type: "INTEGER", nullable: false),
                    Trinium = table.Column<int>(type: "INTEGER", nullable: false),
                    Supplies = table.Column<int>(type: "INTEGER", nullable: false),
                    Energy = table.Column<int>(type: "INTEGER", nullable: false),
                    Personnel = table.Column<int>(type: "INTEGER", nullable: false),
                    Intel = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceStocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceStocks_PlayerBases_PlayerBaseId",
                        column: x => x.PlayerBaseId,
                        principalTable: "PlayerBases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShipyardQueueItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerBaseId = table.Column<int>(type: "INTEGER", nullable: false),
                    ShipType = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletesAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipyardQueueItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShipyardQueueItems_PlayerBases_PlayerBaseId",
                        column: x => x.PlayerBaseId,
                        principalTable: "PlayerBases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpaceCombatMissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AttackerUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    DefenderUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    OriginBaseId = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetBaseId = table.Column<int>(type: "INTEGER", nullable: false),
                    MissionType = table.Column<int>(type: "INTEGER", nullable: false),
                    ShipType = table.Column<int>(type: "INTEGER", nullable: false),
                    ShipCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Distance = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ArrivesAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Naquadah = table.Column<int>(type: "INTEGER", nullable: false),
                    Trinium = table.Column<int>(type: "INTEGER", nullable: false),
                    Supplies = table.Column<int>(type: "INTEGER", nullable: false),
                    Energy = table.Column<int>(type: "INTEGER", nullable: false),
                    Personnel = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpaceCombatMissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpaceCombatMissions_PlayerBases_OriginBaseId",
                        column: x => x.OriginBaseId,
                        principalTable: "PlayerBases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SpaceCombatMissions_PlayerBases_TargetBaseId",
                        column: x => x.TargetBaseId,
                        principalTable: "PlayerBases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SpaceCombatMissions_Users_AttackerUserId",
                        column: x => x.AttackerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpaceCombatMissions_Users_DefenderUserId",
                        column: x => x.DefenderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TradeRoutes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    OriginBaseId = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetBaseId = table.Column<int>(type: "INTEGER", nullable: false),
                    ShipType = table.Column<int>(type: "INTEGER", nullable: false),
                    ShipCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Naquadah = table.Column<int>(type: "INTEGER", nullable: false),
                    Trinium = table.Column<int>(type: "INTEGER", nullable: false),
                    Supplies = table.Column<int>(type: "INTEGER", nullable: false),
                    Energy = table.Column<int>(type: "INTEGER", nullable: false),
                    Personnel = table.Column<int>(type: "INTEGER", nullable: false),
                    IntervalHours = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    NextDueAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastExecutedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeRoutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TradeRoutes_PlayerBases_OriginBaseId",
                        column: x => x.OriginBaseId,
                        principalTable: "PlayerBases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TradeRoutes_PlayerBases_TargetBaseId",
                        column: x => x.TargetBaseId,
                        principalTable: "PlayerBases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TradeRoutes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GateMissionReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    GateMissionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Outcome = table.Column<int>(type: "INTEGER", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", nullable: false),
                    NaquadahFound = table.Column<int>(type: "INTEGER", nullable: false),
                    TriniumFound = table.Column<int>(type: "INTEGER", nullable: false),
                    SuppliesFound = table.Column<int>(type: "INTEGER", nullable: false),
                    IntelFound = table.Column<int>(type: "INTEGER", nullable: false),
                    ArtifactLeadFound = table.Column<bool>(type: "INTEGER", nullable: false),
                    PersonnelLost = table.Column<int>(type: "INTEGER", nullable: false),
                    AnomalyType = table.Column<int>(type: "INTEGER", nullable: true),
                    IsSeasonFocusBonus = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GateMissionReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GateMissionReports_GateMissions_GateMissionId",
                        column: x => x.GateMissionId,
                        principalTable: "GateMissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GateMissionReports_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FleetMovements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    OriginBaseId = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetBaseId = table.Column<int>(type: "INTEGER", nullable: false),
                    DebrisFieldId = table.Column<int>(type: "INTEGER", nullable: true),
                    MissionType = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ShipType = table.Column<int>(type: "INTEGER", nullable: false),
                    ShipCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Naquadah = table.Column<int>(type: "INTEGER", nullable: false),
                    Trinium = table.Column<int>(type: "INTEGER", nullable: false),
                    Supplies = table.Column<int>(type: "INTEGER", nullable: false),
                    Energy = table.Column<int>(type: "INTEGER", nullable: false),
                    Personnel = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ArrivesAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Distance = table.Column<int>(type: "INTEGER", nullable: false),
                    FuelCost = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FleetMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FleetMovements_DebrisFields_DebrisFieldId",
                        column: x => x.DebrisFieldId,
                        principalTable: "DebrisFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FleetMovements_PlayerBases_OriginBaseId",
                        column: x => x.OriginBaseId,
                        principalTable: "PlayerBases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FleetMovements_PlayerBases_TargetBaseId",
                        column: x => x.TargetBaseId,
                        principalTable: "PlayerBases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IntelligenceReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    EspionageMissionId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Body = table.Column<string>(type: "TEXT", nullable: false),
                    DetailDepth = table.Column<int>(type: "INTEGER", nullable: false),
                    IsWarning = table.Column<bool>(type: "INTEGER", nullable: false),
                    WasDetected = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSuspectedDecoy = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntelligenceReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntelligenceReports_EspionageMissions_EspionageMissionId",
                        column: x => x.EspionageMissionId,
                        principalTable: "EspionageMissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PlanetMarketTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlanetMarketOrderId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlanetId = table.Column<int>(type: "INTEGER", nullable: false),
                    SellerUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    BuyerUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    OfferedResource = table.Column<int>(type: "INTEGER", nullable: false),
                    OfferedAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    RequestedResource = table.Column<int>(type: "INTEGER", nullable: false),
                    RequestedAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    FeeAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    FeeRate = table.Column<double>(type: "REAL", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanetMarketTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanetMarketTransactions_PlanetMarketOrders_PlanetMarketOrderId",
                        column: x => x.PlanetMarketOrderId,
                        principalTable: "PlanetMarketOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlanetMarketTransactions_Planets_PlanetId",
                        column: x => x.PlanetId,
                        principalTable: "Planets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanetMarketTransactions_Users_BuyerUserId",
                        column: x => x.BuyerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlanetMarketTransactions_Users_SellerUserId",
                        column: x => x.SellerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TradeReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlanetMarketOrderId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Body = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TradeReports_PlanetMarketOrders_PlanetMarketOrderId",
                        column: x => x.PlanetMarketOrderId,
                        principalTable: "PlanetMarketOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TradeReports_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpaceCombatReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SpaceCombatMissionId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Body = table.Column<string>(type: "TEXT", nullable: false),
                    AttackerWon = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rounds = table.Column<int>(type: "INTEGER", nullable: false),
                    AttackerLosses = table.Column<int>(type: "INTEGER", nullable: false),
                    DefenderLosses = table.Column<int>(type: "INTEGER", nullable: false),
                    LootNaquadah = table.Column<int>(type: "INTEGER", nullable: false),
                    LootTrinium = table.Column<int>(type: "INTEGER", nullable: false),
                    LootSupplies = table.Column<int>(type: "INTEGER", nullable: false),
                    LootEnergy = table.Column<int>(type: "INTEGER", nullable: false),
                    LootPersonnel = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpaceCombatReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpaceCombatReports_SpaceCombatMissions_SpaceCombatMissionId",
                        column: x => x.SpaceCombatMissionId,
                        principalTable: "SpaceCombatMissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FleetReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    FleetMovementId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Body = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FleetReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FleetReports_FleetMovements_FleetMovementId",
                        column: x => x.FleetMovementId,
                        principalTable: "FleetMovements",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FleetReports_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AchievementProgresses_UserId_AchievementKey",
                table: "AchievementProgresses",
                columns: new[] { "UserId", "AchievementKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AllianceApplications_AllianceId_UserId_AcceptedAtUtc_RejectedAtUtc",
                table: "AllianceApplications",
                columns: new[] { "AllianceId", "UserId", "AcceptedAtUtc", "RejectedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AllianceApplications_UserId",
                table: "AllianceApplications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AllianceDiplomacyStatuses_AllianceAId_AllianceBId",
                table: "AllianceDiplomacyStatuses",
                columns: new[] { "AllianceAId", "AllianceBId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AllianceDiplomacyStatuses_AllianceBId",
                table: "AllianceDiplomacyStatuses",
                column: "AllianceBId");

            migrationBuilder.CreateIndex(
                name: "IX_AllianceMembers_AllianceId",
                table: "AllianceMembers",
                column: "AllianceId");

            migrationBuilder.CreateIndex(
                name: "IX_AllianceMembers_MentorUserId",
                table: "AllianceMembers",
                column: "MentorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AllianceMembers_UserId",
                table: "AllianceMembers",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Alliances_FounderUserId",
                table: "Alliances",
                column: "FounderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Alliances_ServerId_Name",
                table: "Alliances",
                columns: new[] { "ServerId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Alliances_ServerId_Tag",
                table: "Alliances",
                columns: new[] { "ServerId", "Tag" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AllianceWarGoals_AllianceId",
                table: "AllianceWarGoals",
                column: "AllianceId");

            migrationBuilder.CreateIndex(
                name: "IX_AllianceWarGoals_PlanetId",
                table: "AllianceWarGoals",
                column: "PlanetId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseShips_PlayerBaseId",
                table: "BaseShips",
                column: "PlayerBaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BuildingLevels_PlayerBaseId",
                table: "BuildingLevels",
                column: "PlayerBaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BuildQueueItems_PlayerBaseId",
                table: "BuildQueueItems",
                column: "PlayerBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSkills_UserId",
                table: "CharacterSkills",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractProgresses_UserId_ContractKey_PeriodStartUtc",
                table: "ContractProgresses",
                columns: new[] { "UserId", "ContractKey", "PeriodStartUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DebrisFields_PlayerBaseId",
                table: "DebrisFields",
                column: "PlayerBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_DecoyProfiles_PlayerBaseId",
                table: "DecoyProfiles",
                column: "PlayerBaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DefenseUnits_LocalCombatMissionId",
                table: "DefenseUnits",
                column: "LocalCombatMissionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DefenseUnits_LocalCombatMissionId1",
                table: "DefenseUnits",
                column: "LocalCombatMissionId1");

            migrationBuilder.CreateIndex(
                name: "IX_EspionageMissions_SourceBaseId",
                table: "EspionageMissions",
                column: "SourceBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_EspionageMissions_TargetBaseId",
                table: "EspionageMissions",
                column: "TargetBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_EspionageMissions_UserId",
                table: "EspionageMissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Factions_Name",
                table: "Factions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FleetMovements_DebrisFieldId",
                table: "FleetMovements",
                column: "DebrisFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_FleetMovements_OriginBaseId",
                table: "FleetMovements",
                column: "OriginBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_FleetMovements_TargetBaseId",
                table: "FleetMovements",
                column: "TargetBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_FleetReports_FleetMovementId",
                table: "FleetReports",
                column: "FleetMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_FleetReports_UserId",
                table: "FleetReports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameServers_Slug",
                table: "GameServers",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GateAddresses_PlanetId",
                table: "GateAddresses",
                column: "PlanetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GateAddresses_ServerId_Code",
                table: "GateAddresses",
                columns: new[] { "ServerId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GateMissionReports_GateMissionId",
                table: "GateMissionReports",
                column: "GateMissionId");

            migrationBuilder.CreateIndex(
                name: "IX_GateMissionReports_UserId",
                table: "GateMissionReports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GateMissions_GateAddressId",
                table: "GateMissions",
                column: "GateAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_GateMissions_MissionTeamId",
                table: "GateMissions",
                column: "MissionTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_GateMissions_UserId",
                table: "GateMissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroundUnits_LocalCombatMissionId",
                table: "GroundUnits",
                column: "LocalCombatMissionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroundUnits_LocalCombatMissionId1",
                table: "GroundUnits",
                column: "LocalCombatMissionId1");

            migrationBuilder.CreateIndex(
                name: "IX_IntelligenceReports_EspionageMissionId",
                table: "IntelligenceReports",
                column: "EspionageMissionId");

            migrationBuilder.CreateIndex(
                name: "IX_KnownGateAddresses_GateAddressId",
                table: "KnownGateAddresses",
                column: "GateAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_KnownGateAddresses_UserId_GateAddressId",
                table: "KnownGateAddresses",
                columns: new[] { "UserId", "GateAddressId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalActionReports_PlanetSectorId",
                table: "LocalActionReports",
                column: "PlanetSectorId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalActionReports_UserId",
                table: "LocalActionReports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalCombatMissions_AttackerUserId",
                table: "LocalCombatMissions",
                column: "AttackerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalCombatMissions_DefenderUserId",
                table: "LocalCombatMissions",
                column: "DefenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalCombatMissions_PlanetSectorId",
                table: "LocalCombatMissions",
                column: "PlanetSectorId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalCombatRounds_LocalCombatMissionId",
                table: "LocalCombatRounds",
                column: "LocalCombatMissionId");

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_ServerId_IpHash_AttemptedAtUtc",
                table: "LoginAttempts",
                columns: new[] { "ServerId", "IpHash", "AttemptedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_ServerId_UsernameKey_AttemptedAtUtc",
                table: "LoginAttempts",
                columns: new[] { "ServerId", "UsernameKey", "AttemptedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_MissionTeams_UserId",
                table: "MissionTeams",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanetMarketOrders_PlanetId_CompletedAtUtc_CancelledAtUtc_ExpiresAtUtc",
                table: "PlanetMarketOrders",
                columns: new[] { "PlanetId", "CompletedAtUtc", "CancelledAtUtc", "ExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PlanetMarketOrders_SellerBaseId",
                table: "PlanetMarketOrders",
                column: "SellerBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanetMarketOrders_SellerUserId",
                table: "PlanetMarketOrders",
                column: "SellerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanetMarketTransactions_BuyerUserId",
                table: "PlanetMarketTransactions",
                column: "BuyerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanetMarketTransactions_PlanetId",
                table: "PlanetMarketTransactions",
                column: "PlanetId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanetMarketTransactions_PlanetMarketOrderId",
                table: "PlanetMarketTransactions",
                column: "PlanetMarketOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanetMarketTransactions_SellerUserId",
                table: "PlanetMarketTransactions",
                column: "SellerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Planets_ServerId_Name",
                table: "Planets",
                columns: new[] { "ServerId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanetSectors_PlanetId_Number",
                table: "PlanetSectors",
                columns: new[] { "PlanetId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerBases_FactionId",
                table: "PlayerBases",
                column: "FactionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerBases_PlanetSectorId",
                table: "PlayerBases",
                column: "PlanetSectorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerBases_UserId",
                table: "PlayerBases",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMessages_RecipientUserId_IsDeletedByRecipient_CreatedAtUtc",
                table: "PlayerMessages",
                columns: new[] { "RecipientUserId", "IsDeletedByRecipient", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMessages_SenderUserId_IsDeletedBySender_CreatedAtUtc",
                table: "PlayerMessages",
                columns: new[] { "SenderUserId", "IsDeletedBySender", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerProtectionStatuses_UserId",
                table: "PlayerProtectionStatuses",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestlineStepProgresses_UserId_StepKey",
                table: "QuestlineStepProgresses",
                columns: new[] { "UserId", "StepKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_UserId",
                table: "Reports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchLevels_UserId",
                table: "ResearchLevels",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResearchQueueItems_UserId",
                table: "ResearchQueueItems",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceStocks_PlayerBaseId",
                table: "ResourceStocks",
                column: "PlayerBaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SectorBattleReports_LocalCombatMissionId",
                table: "SectorBattleReports",
                column: "LocalCombatMissionId");

            migrationBuilder.CreateIndex(
                name: "IX_SectorBattleReports_PlanetSectorId",
                table: "SectorBattleReports",
                column: "PlanetSectorId");

            migrationBuilder.CreateIndex(
                name: "IX_SectorBattleReports_UserId",
                table: "SectorBattleReports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SectorClaims_PlanetSectorId_IsCompleted",
                table: "SectorClaims",
                columns: new[] { "PlanetSectorId", "IsCompleted" });

            migrationBuilder.CreateIndex(
                name: "IX_SectorClaims_UserId",
                table: "SectorClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SectorControls_PlanetSectorId",
                table: "SectorControls",
                column: "PlanetSectorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SectorControls_UserId",
                table: "SectorControls",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipyardQueueItems_PlayerBaseId",
                table: "ShipyardQueueItems",
                column: "PlayerBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_SpaceCombatMissions_AttackerUserId",
                table: "SpaceCombatMissions",
                column: "AttackerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SpaceCombatMissions_DefenderUserId",
                table: "SpaceCombatMissions",
                column: "DefenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SpaceCombatMissions_OriginBaseId",
                table: "SpaceCombatMissions",
                column: "OriginBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_SpaceCombatMissions_TargetBaseId",
                table: "SpaceCombatMissions",
                column: "TargetBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_SpaceCombatReports_SpaceCombatMissionId",
                table: "SpaceCombatReports",
                column: "SpaceCombatMissionId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeReports_PlanetMarketOrderId",
                table: "TradeReports",
                column: "PlanetMarketOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeReports_UserId",
                table: "TradeReports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeRoutes_OriginBaseId",
                table: "TradeRoutes",
                column: "OriginBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeRoutes_TargetBaseId",
                table: "TradeRoutes",
                column: "TargetBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeRoutes_UserId",
                table: "TradeRoutes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_FactionId",
                table: "Users",
                column: "FactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsNpc",
                table: "Users",
                column: "IsNpc");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ServerId_Email",
                table: "Users",
                columns: new[] { "ServerId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ServerId_UserName",
                table: "Users",
                columns: new[] { "ServerId", "UserName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorldEventContributions_UserId",
                table: "WorldEventContributions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorldEventContributions_WorldEventId_UserId",
                table: "WorldEventContributions",
                columns: new[] { "WorldEventId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorldEvents_ServerId_Status",
                table: "WorldEvents",
                columns: new[] { "ServerId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AchievementProgresses");

            migrationBuilder.DropTable(
                name: "AllianceApplications");

            migrationBuilder.DropTable(
                name: "AllianceDiplomacyStatuses");

            migrationBuilder.DropTable(
                name: "AllianceMembers");

            migrationBuilder.DropTable(
                name: "AllianceWarGoals");

            migrationBuilder.DropTable(
                name: "BaseShips");

            migrationBuilder.DropTable(
                name: "BuildingLevels");

            migrationBuilder.DropTable(
                name: "BuildQueueItems");

            migrationBuilder.DropTable(
                name: "CharacterSkills");

            migrationBuilder.DropTable(
                name: "ContractProgresses");

            migrationBuilder.DropTable(
                name: "DecoyProfiles");

            migrationBuilder.DropTable(
                name: "DefenseUnits");

            migrationBuilder.DropTable(
                name: "FleetReports");

            migrationBuilder.DropTable(
                name: "GateMissionReports");

            migrationBuilder.DropTable(
                name: "GroundUnits");

            migrationBuilder.DropTable(
                name: "IntelligenceReports");

            migrationBuilder.DropTable(
                name: "KnownGateAddresses");

            migrationBuilder.DropTable(
                name: "LocalActionReports");

            migrationBuilder.DropTable(
                name: "LocalCombatRounds");

            migrationBuilder.DropTable(
                name: "LoginAttempts");

            migrationBuilder.DropTable(
                name: "PlanetMarketTransactions");

            migrationBuilder.DropTable(
                name: "PlayerMessages");

            migrationBuilder.DropTable(
                name: "PlayerProtectionStatuses");

            migrationBuilder.DropTable(
                name: "QuestlineStepProgresses");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "ResearchLevels");

            migrationBuilder.DropTable(
                name: "ResearchQueueItems");

            migrationBuilder.DropTable(
                name: "ResourceStocks");

            migrationBuilder.DropTable(
                name: "SectorBattleReports");

            migrationBuilder.DropTable(
                name: "SectorClaims");

            migrationBuilder.DropTable(
                name: "SectorControls");

            migrationBuilder.DropTable(
                name: "ShipyardQueueItems");

            migrationBuilder.DropTable(
                name: "SpaceCombatReports");

            migrationBuilder.DropTable(
                name: "TradeReports");

            migrationBuilder.DropTable(
                name: "TradeRoutes");

            migrationBuilder.DropTable(
                name: "TradeTaxRules");

            migrationBuilder.DropTable(
                name: "WorldEventContributions");

            migrationBuilder.DropTable(
                name: "Alliances");

            migrationBuilder.DropTable(
                name: "FleetMovements");

            migrationBuilder.DropTable(
                name: "GateMissions");

            migrationBuilder.DropTable(
                name: "EspionageMissions");

            migrationBuilder.DropTable(
                name: "LocalCombatMissions");

            migrationBuilder.DropTable(
                name: "SpaceCombatMissions");

            migrationBuilder.DropTable(
                name: "PlanetMarketOrders");

            migrationBuilder.DropTable(
                name: "WorldEvents");

            migrationBuilder.DropTable(
                name: "DebrisFields");

            migrationBuilder.DropTable(
                name: "GateAddresses");

            migrationBuilder.DropTable(
                name: "MissionTeams");

            migrationBuilder.DropTable(
                name: "PlayerBases");

            migrationBuilder.DropTable(
                name: "PlanetSectors");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Planets");

            migrationBuilder.DropTable(
                name: "Factions");

            migrationBuilder.DropTable(
                name: "GameServers");
        }
    }
}
