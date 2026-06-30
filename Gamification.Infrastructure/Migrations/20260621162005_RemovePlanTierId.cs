using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gamification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovePlanTierId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlanTierId",
                schema: "gamification",
                table: "ExplorerProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PlanTierId",
                schema: "gamification",
                table: "ExplorerProfiles",
                type: "uuid",
                nullable: true);
        }
    }
}
