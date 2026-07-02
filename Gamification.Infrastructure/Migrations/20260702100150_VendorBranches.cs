using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gamification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VendorBranches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_XpTransactions_ExplorerProfileId_Source_ReferenceId",
                schema: "gamification",
                table: "XpTransactions",
                columns: new[] { "ExplorerProfileId", "Source", "ReferenceId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_XpTransactions_ExplorerProfileId_Source_ReferenceId",
                schema: "gamification",
                table: "XpTransactions");
        }
    }
}
