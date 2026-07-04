using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gamification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VendorProfileRemoveId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                schema: "gamification",
                table: "VendorProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                schema: "gamification",
                table: "VendorProfiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
