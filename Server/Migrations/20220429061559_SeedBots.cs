using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    public partial class SeedBots : Migration
    {
	    protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DailyLosses",
                table: "Player",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DailyWins",
                table: "Player",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Elo",
                table: "Player",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyLosses",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "DailyWins",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Elo",
                table: "Player");
        }
    }
}
