using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rewards.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeneratedPlanJson",
                schema: "rewards",
                table: "TravelPlans");

            migrationBuilder.AlterColumn<decimal>(
                name: "BudgetLimit",
                schema: "rewards",
                table: "TravelPlans",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<string>(
                name: "GeneratedPlan",
                schema: "rewards",
                table: "TravelPlans",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeneratedPlan",
                schema: "rewards",
                table: "TravelPlans");

            migrationBuilder.AlterColumn<decimal>(
                name: "BudgetLimit",
                schema: "rewards",
                table: "TravelPlans",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<string>(
                name: "GeneratedPlanJson",
                schema: "rewards",
                table: "TravelPlans",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }
    }
}
