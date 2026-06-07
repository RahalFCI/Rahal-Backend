using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gamification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class achievementCriteriaTypeAddCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExplorerAchievement_ExplorerProfiles_ProfileUserId",
                schema: "gamification",
                table: "ExplorerAchievement");

            migrationBuilder.DropIndex(
                name: "IX_ExplorerAchievement_ProfileUserId",
                schema: "gamification",
                table: "ExplorerAchievement");

            migrationBuilder.DropColumn(
                name: "ProfileUserId",
                schema: "gamification",
                table: "ExplorerAchievement");

            migrationBuilder.AddColumn<Guid>(
                name: "ExplorerId",
                schema: "gamification",
                table: "CheckInChallenges",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "gamification",
                table: "AchievementCriteriaTypes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_ExplorerAchievement_ExplorerProfiles_ExplorerId",
                schema: "gamification",
                table: "ExplorerAchievement",
                column: "ExplorerId",
                principalSchema: "gamification",
                principalTable: "ExplorerProfiles",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExplorerAchievement_ExplorerProfiles_ExplorerId",
                schema: "gamification",
                table: "ExplorerAchievement");

            migrationBuilder.DropColumn(
                name: "ExplorerId",
                schema: "gamification",
                table: "CheckInChallenges");

            migrationBuilder.DropColumn(
                name: "Code",
                schema: "gamification",
                table: "AchievementCriteriaTypes");

            migrationBuilder.AddColumn<Guid>(
                name: "ProfileUserId",
                schema: "gamification",
                table: "ExplorerAchievement",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExplorerAchievement_ProfileUserId",
                schema: "gamification",
                table: "ExplorerAchievement",
                column: "ProfileUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExplorerAchievement_ExplorerProfiles_ProfileUserId",
                schema: "gamification",
                table: "ExplorerAchievement",
                column: "ProfileUserId",
                principalSchema: "gamification",
                principalTable: "ExplorerProfiles",
                principalColumn: "UserId");
        }
    }
}
