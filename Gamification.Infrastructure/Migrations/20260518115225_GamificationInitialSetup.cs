using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gamification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GamificationInitialSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "gamification");

            migrationBuilder.CreateTable(
                name: "AchievementCriteriaTypes",
                schema: "gamification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementCriteriaTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Badges",
                schema: "gamification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", maxLength: 500, nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Badges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Challenges",
                schema: "gamification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Difficulty = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    MinimumLevelRequired = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    XpReward = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Challenges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExplorerProfiles",
                schema: "gamification",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProfilePictureURL = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Gender = table.Column<string>(type: "text", nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Bio = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsPremium = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PlanTierId = table.Column<Guid>(type: "uuid", nullable: true),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExplorerProfiles", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "VendorCategories",
                schema: "gamification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryName = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Achievements",
                schema: "gamification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    BadgeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Xp = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AchievementCriteriaTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriteriaThreshold = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Achievements_AchievementCriteriaTypes_AchievementCriteriaTy~",
                        column: x => x.AchievementCriteriaTypeId,
                        principalSchema: "gamification",
                        principalTable: "AchievementCriteriaTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Achievements_Badges_BadgeId",
                        column: x => x.BadgeId,
                        principalSchema: "gamification",
                        principalTable: "Badges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CheckInChallenges",
                schema: "gamification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChallengeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckInId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProofUrl = table.Column<string>(type: "text", nullable: false),
                    ValidationStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckInChallenges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckInChallenges_Challenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalSchema: "gamification",
                        principalTable: "Challenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserStats",
                schema: "gamification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExplorerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    AvailableXp = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CumulativeXp = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CurrentStreak = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastActivityDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalCheckInCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalAchievementCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalChallengeCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalBadgeCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LongestStreak = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserStats_ExplorerProfiles_ExplorerProfileId",
                        column: x => x.ExplorerProfileId,
                        principalSchema: "gamification",
                        principalTable: "ExplorerProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "XpTransactions",
                schema: "gamification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExplorerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XpTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_XpTransactions_ExplorerProfiles_ExplorerProfileId",
                        column: x => x.ExplorerProfileId,
                        principalSchema: "gamification",
                        principalTable: "ExplorerProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VendorProfiles",
                schema: "gamification",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProfilePictureURL = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AddressUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    WorkingHours = table.Column<string>(type: "text", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorProfiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_VendorProfiles_VendorCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "gamification",
                        principalTable: "VendorCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExplorerAchievement",
                schema: "gamification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AchievementId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExplorerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EarnedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsNotified = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExplorerAchievement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExplorerAchievement_Achievements_AchievementId",
                        column: x => x.AchievementId,
                        principalSchema: "gamification",
                        principalTable: "Achievements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExplorerAchievement_ExplorerProfiles_ProfileUserId",
                        column: x => x.ProfileUserId,
                        principalSchema: "gamification",
                        principalTable: "ExplorerProfiles",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AchievementCriteriaTypes_Name",
                schema: "gamification",
                table: "AchievementCriteriaTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_AchievementCriteriaTypeId",
                schema: "gamification",
                table: "Achievements",
                column: "AchievementCriteriaTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_BadgeId",
                schema: "gamification",
                table: "Achievements",
                column: "BadgeId");

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_Title",
                schema: "gamification",
                table: "Achievements",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Badges_IsDeleted",
                schema: "gamification",
                table: "Badges",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Badges_Name",
                schema: "gamification",
                table: "Badges",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_Difficulty",
                schema: "gamification",
                table: "Challenges",
                column: "Difficulty");

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_Name",
                schema: "gamification",
                table: "Challenges",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_Type",
                schema: "gamification",
                table: "Challenges",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_CheckInChallenges_ChallengeId",
                schema: "gamification",
                table: "CheckInChallenges",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckInChallenges_CheckInId",
                schema: "gamification",
                table: "CheckInChallenges",
                column: "CheckInId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckInChallenges_ValidationStatus",
                schema: "gamification",
                table: "CheckInChallenges",
                column: "ValidationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ExplorerAchievement_ProfileUserId",
                schema: "gamification",
                table: "ExplorerAchievement",
                column: "ProfileUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExplorerAchievements_AchievementId",
                schema: "gamification",
                table: "ExplorerAchievement",
                column: "AchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_ExplorerAchievements_ExplorerProfileId",
                schema: "gamification",
                table: "ExplorerAchievement",
                column: "ExplorerId");

            migrationBuilder.CreateIndex(
                name: "IX_ExplorerProfiles_IsDeleted",
                schema: "gamification",
                table: "ExplorerProfiles",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ExplorerProfiles_UserId",
                schema: "gamification",
                table: "ExplorerProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserStats_ExplorerProfileId",
                schema: "gamification",
                table: "UserStats",
                column: "ExplorerProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorCategory_CategoryName",
                schema: "gamification",
                table: "VendorCategories",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorCategory_IsDeleted",
                schema: "gamification",
                table: "VendorCategories",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_VendorProfile_CategoryId",
                schema: "gamification",
                table: "VendorProfiles",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorProfiles_IsDeleted",
                schema: "gamification",
                table: "VendorProfiles",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_VendorProfiles_UserId",
                schema: "gamification",
                table: "VendorProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_XpTransactions_ExplorerProfileId",
                schema: "gamification",
                table: "XpTransactions",
                column: "ExplorerProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckInChallenges",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "ExplorerAchievement",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "UserStats",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "VendorProfiles",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "XpTransactions",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "Challenges",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "Achievements",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "VendorCategories",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "ExplorerProfiles",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "AchievementCriteriaTypes",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "Badges",
                schema: "gamification");
        }
    }
}
