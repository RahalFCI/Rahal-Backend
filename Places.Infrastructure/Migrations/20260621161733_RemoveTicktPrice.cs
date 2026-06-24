using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Places.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTicktPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TicketPrice",
                schema: "places",
                table: "Places");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "TicketPrice",
                schema: "places",
                table: "Places",
                type: "double precision",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
