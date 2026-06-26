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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VendorBranches",
                schema: "gamification");
        }
    }
}
