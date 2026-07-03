using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentTransactionQueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Payments_CreatedAt",
                schema: "payment",
                table: "Payments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Currency_CreatedAt",
                schema: "payment",
                table: "Payments",
                columns: new[] { "Currency", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ExplorerId_CreatedAt",
                schema: "payment",
                table: "Payments",
                columns: new[] { "ExplorerId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status_CreatedAt",
                schema: "payment",
                table: "Payments",
                columns: new[] { "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_CreatedAt",
                schema: "payment",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_Currency_CreatedAt",
                schema: "payment",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ExplorerId_CreatedAt",
                schema: "payment",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_Status_CreatedAt",
                schema: "payment",
                table: "Payments");
        }
    }
}
