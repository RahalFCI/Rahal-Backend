using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rewards.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionDurationAndTotalCost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Duration",
                schema: "rewards",
                table: "Subscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCost",
                schema: "rewards",
                table: "Subscriptions",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(
                """
                UPDATE rewards."Subscriptions" AS s
                SET "TotalCost" = CASE
                    WHEN s."PaymentMethod" = 'Xp' THEN p."WeeklyXpCost"::numeric
                    ELSE p."WeeklyPrice"
                END
                FROM rewards."PlanTiers" AS p
                WHERE s."PlanTierId" = p."Id";
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
