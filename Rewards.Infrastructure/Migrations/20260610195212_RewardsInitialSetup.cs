using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rewards.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RewardsInitialSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "rewards");

            migrationBuilder.CreateTable(
                name: "Coupons",
                schema: "rewards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    XpCost = table.Column<int>(type: "integer", nullable: false),
                    DiscountType = table.Column<string>(type: "text", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxDiscountValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MinimumCharge = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxClaims = table.Column<int>(type: "integer", nullable: false),
                    CurrentClaims = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanTiers",
                schema: "rewards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    WeeklyPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    WeeklyXpCost = table.Column<int>(type: "integer", nullable: false),
                    XpMultiplier = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    MaxTravelPlans = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanTiers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserCoupons",
                schema: "rewards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExplorerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CouponId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsRedeemed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ClaimedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RedeemedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCoupons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCoupons_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalSchema: "rewards",
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                schema: "rewards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExplorerId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanTierId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentMethod = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_PlanTiers_PlanTierId",
                        column: x => x.PlanTierId,
                        principalSchema: "rewards",
                        principalTable: "PlanTiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TravelPlans",
                schema: "rewards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExplorerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    BudgetLimit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    StayDurationDays = table.Column<int>(type: "integer", nullable: false),
                    Prompt = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    GeneratedPlanJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TravelPlans_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalSchema: "rewards",
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_DiscountType",
                schema: "rewards",
                table: "Coupons",
                column: "DiscountType");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_ExpiresAt",
                schema: "rewards",
                table: "Coupons",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_IsActive",
                schema: "rewards",
                table: "Coupons",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_VendorId",
                schema: "rewards",
                table: "Coupons",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanTiers_IsActive",
                schema: "rewards",
                table: "PlanTiers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PlanTiers_Name",
                schema: "rewards",
                table: "PlanTiers",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_ExpiresAt",
                schema: "rewards",
                table: "Subscriptions",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_ExplorerId",
                schema: "rewards",
                table: "Subscriptions",
                column: "ExplorerId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_ExplorerId_Status",
                schema: "rewards",
                table: "Subscriptions",
                columns: new[] { "ExplorerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PlanTierId",
                schema: "rewards",
                table: "Subscriptions",
                column: "PlanTierId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_Status",
                schema: "rewards",
                table: "Subscriptions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TravelPlans_ExplorerId",
                schema: "rewards",
                table: "TravelPlans",
                column: "ExplorerId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelPlans_SubscriptionId",
                schema: "rewards",
                table: "TravelPlans",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCoupons_Code",
                schema: "rewards",
                table: "UserCoupons",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCoupons_CouponId",
                schema: "rewards",
                table: "UserCoupons",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCoupons_ExplorerId_CouponId",
                schema: "rewards",
                table: "UserCoupons",
                columns: new[] { "ExplorerId", "CouponId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCoupons_Status",
                schema: "rewards",
                table: "UserCoupons",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TravelPlans",
                schema: "rewards");

            migrationBuilder.DropTable(
                name: "UserCoupons",
                schema: "rewards");

            migrationBuilder.DropTable(
                name: "Subscriptions",
                schema: "rewards");

            migrationBuilder.DropTable(
                name: "Coupons",
                schema: "rewards");

            migrationBuilder.DropTable(
                name: "PlanTiers",
                schema: "rewards");
        }
    }
}
