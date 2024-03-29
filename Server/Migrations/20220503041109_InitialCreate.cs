﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Player",
                columns: table =>
                    new
                    {
                        Id = table.Column<Guid>(type: "uuid", nullable: false),
                        Name = table.Column<string>(type: "text", nullable: false),
                        Elo = table.Column<int>(type: "integer", nullable: false),
                        DailyWinStreak = table.Column<int>(type: "integer", nullable: false),
                        MaxDailyWinStreak = table.Column<int>(type: "integer", nullable: false),
                        DailyWins = table.Column<int>(type: "integer", nullable: false),
                        DailyLosses = table.Column<int>(type: "integer", nullable: false)
                    },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Player", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "GameResult",
                columns: table =>
                    new
                    {
                        Id = table.Column<Guid>(type: "uuid", nullable: false),
                        WinnerId = table.Column<Guid>(type: "uuid", nullable: false),
                        LoserId = table.Column<Guid>(type: "uuid", nullable: false),
                        Turns = table.Column<int>(type: "integer", nullable: false),
                        LostBy = table.Column<int>(type: "integer", nullable: false),
                        Daily = table.Column<bool>(type: "boolean", nullable: false),
                        DailyIndex = table.Column<int>(type: "integer", nullable: false)
                    },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameResult_Player_LoserId",
                        column: x => x.LoserId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_GameResult_Player_WinnerId",
                        column: x => x.WinnerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_GameResult_LoserId",
                table: "GameResult",
                column: "LoserId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_GameResult_WinnerId",
                table: "GameResult",
                column: "WinnerId"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GameResult");

            migrationBuilder.DropTable(name: "Player");
        }
    }
}
