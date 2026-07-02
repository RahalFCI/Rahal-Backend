using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gamification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorBranches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The original 20260625120000_AddVendorBranches migration shipped without
            // its Designer file, so EF never recognised it and the table was never
            // created — yet the model snapshot already listed VendorBranch, so the
            // regenerated diff omitted the CreateTable. Re-add it here so the table
            // is actually created.
            migrationBuilder.CreateTable(
                name: "VendorBranches",
                schema: "gamification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorBranches", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VendorBranches_PlaceId",
                schema: "gamification",
                table: "VendorBranches",
                column: "PlaceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorBranches_VendorId",
                schema: "gamification",
                table: "VendorBranches",
                column: "VendorId");

            // Use IF NOT EXISTS to be safe if the index was partially created before
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_XpTransactions_ExplorerProfileId_Source_ReferenceId""
                ON gamification.""XpTransactions"" (""ExplorerProfileId"", ""Source"", ""ReferenceId"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_XpTransactions_ExplorerProfileId_Source_ReferenceId",
                schema: "gamification",
                table: "XpTransactions");

            migrationBuilder.DropTable(
                name: "VendorBranches",
                schema: "gamification");
        }
    }
}
