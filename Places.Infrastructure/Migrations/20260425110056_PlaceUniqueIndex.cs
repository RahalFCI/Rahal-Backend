using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Places.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PlaceUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Places_Name_Latitude_Longitude",
                schema: "places",
                table: "Places",
                columns: new[] { "Name", "Latitude", "Longitude" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Places_Name_Latitude_Longitude",
                schema: "places",
                table: "Places");
        }
    }
}
