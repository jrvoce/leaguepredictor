using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AustoPredictor.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPredictionDesignation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Designation",
                table: "PlayerPredictions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Designation",
                table: "PlayerPredictions");
        }
    }
}
