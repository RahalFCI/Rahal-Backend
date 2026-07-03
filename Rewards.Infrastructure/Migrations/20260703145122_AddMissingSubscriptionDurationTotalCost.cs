using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rewards.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingSubscriptionDurationTotalCost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE rewards."Subscriptions"
                ADD COLUMN IF NOT EXISTS "Duration" integer NOT NULL DEFAULT 1;

                ALTER TABLE rewards."Subscriptions"
                ADD COLUMN IF NOT EXISTS "TotalCost" numeric(18,2) NOT NULL DEFAULT 0;

                UPDATE rewards."Subscriptions" AS s
                SET "TotalCost" = CASE
                    WHEN s."PaymentMethod" = 'Xp' THEN p."WeeklyXpCost"::numeric * s."Duration"
                    ELSE p."WeeklyPrice" * s."Duration"
                END
                FROM rewards."PlanTiers" AS p
                WHERE s."PlanTierId" = p."Id"
                  AND s."TotalCost" = 0;
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                schema: "rewards",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "TotalCost",
                schema: "rewards",
                table: "Subscriptions");
        }
    }
}
