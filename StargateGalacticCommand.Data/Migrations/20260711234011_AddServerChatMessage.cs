using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StargateGalacticCommand.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddServerChatMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SpecialResourceAmount",
                table: "GateMissionReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SpecialResourceFound",
                table: "GateMissionReports",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PlayerBaseSpecialResources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerBaseId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerBaseSpecialResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerBaseSpecialResources_PlayerBases_PlayerBaseId",
                        column: x => x.PlayerBaseId,
                        principalTable: "PlayerBases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServerChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Body = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServerChatMessages_GameServers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "GameServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServerChatMessages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerBaseSpecialResources_PlayerBaseId_Type",
                table: "PlayerBaseSpecialResources",
                columns: new[] { "PlayerBaseId", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServerChatMessages_ServerId_CreatedAtUtc",
                table: "ServerChatMessages",
                columns: new[] { "ServerId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ServerChatMessages_UserId",
                table: "ServerChatMessages",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerBaseSpecialResources");

            migrationBuilder.DropTable(
                name: "ServerChatMessages");

            migrationBuilder.DropColumn(
                name: "SpecialResourceAmount",
                table: "GateMissionReports");

            migrationBuilder.DropColumn(
                name: "SpecialResourceFound",
                table: "GateMissionReports");
        }
    }
}
