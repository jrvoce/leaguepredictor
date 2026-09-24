using LeaguePredictor.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaguePredictor.Data.Migrations;

[DbContext(typeof(LeaguePredictorDbContext))]
[Migration("20260924120000_AddPlayerBonusPredictions")]
public partial class AddPlayerBonusPredictions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PlayerBonusPredictions",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                PlayerId = table.Column<int>(type: "int", nullable: false),
                Type = table.Column<int>(type: "int", nullable: false),
                Value = table.Column<int>(type: "int", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlayerBonusPredictions", x => x.Id);
                table.ForeignKey(
                    name: "FK_PlayerBonusPredictions_Players_PlayerId",
                    column: x => x.PlayerId,
                    principalTable: "Players",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PlayerBonusPredictions_PlayerId_Type",
            table: "PlayerBonusPredictions",
            columns: new[] { "PlayerId", "Type" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PlayerBonusPredictions");
    }
}
