using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Places.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PlaceUniqueIndexModification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Places_Name",
                schema: "places",
                table: "Places");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Places_Name",
                schema: "places",
                table: "Places",
                column: "Name");
        }
    }
}
