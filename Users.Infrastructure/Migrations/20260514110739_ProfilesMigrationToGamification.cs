using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Users.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProfilesMigrationToGamification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminProfiles",
                schema: "users");

            migrationBuilder.DropTable(
                name: "ExplorerProfiles",
                schema: "users");

            migrationBuilder.DropTable(
                name: "VendorProfiles",
                schema: "users");

            migrationBuilder.DropTable(
                name: "VendorCategories",
                schema: "users");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                schema: "users",
                table: "EmailVerificationTokens",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                schema: "users",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                schema: "users",
                table: "EmailVerificationTokens",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                schema: "users",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.CreateTable(
                name: "AdminProfiles",
                schema: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "users",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExplorerProfiles",
                schema: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AvailableXp = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Bio = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CumulativeXp = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPremium = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Level = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    PlanTierId = table.Column<int>(type: "integer", nullable: true),
                    Streak = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExplorerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExplorerProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "users",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VendorCategories",
                schema: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryName = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VendorProfiles",
                schema: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AddressUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WorkingHours = table.Column<Dictionary<DayOfWeek, string>>(type: "jsonb", nullable: false, comment: "JSON format: {\"Monday\": \"09:00-17:00\", ...}")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "users",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VendorProfiles_VendorCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "users",
                        principalTable: "VendorCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminProfiles_UserId",
                schema: "users",
                table: "AdminProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExplorerProfiles_IsDeleted",
                schema: "users",
                table: "ExplorerProfiles",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ExplorerProfiles_UserId",
                schema: "users",
                table: "ExplorerProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorProfiles_CategoryId",
                schema: "users",
                table: "VendorProfiles",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorProfiles_IsApproved",
                schema: "users",
                table: "VendorProfiles",
                column: "IsApproved");

            migrationBuilder.CreateIndex(
                name: "IX_VendorProfiles_IsDeleted",
                schema: "users",
                table: "VendorProfiles",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_VendorProfiles_UserId",
                schema: "users",
                table: "VendorProfiles",
                column: "UserId",
                unique: true);
        }
    }
}
