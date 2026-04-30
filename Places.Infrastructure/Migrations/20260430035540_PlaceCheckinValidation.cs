using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Places.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PlaceCheckinValidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                schema: "places",
                table: "CheckIns",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                schema: "places",
                table: "CheckIns",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "RiskScore",
                schema: "places",
                table: "CheckIns",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                schema: "places",
                table: "CheckIns");

            migrationBuilder.DropColumn(
                name: "Longitude",
                schema: "places",
                table: "CheckIns");

            migrationBuilder.DropColumn(
                name: "RiskScore",
                schema: "places",
                table: "CheckIns");
        }
    }
}
