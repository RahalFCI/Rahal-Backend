using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gamification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VendorBranches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Deduplicate rows that would violate the unique index created by AddVendorBranches
            migrationBuilder.Sql(@"
                DELETE FROM gamification.""XpTransactions""
                WHERE ""Id"" NOT IN (
                    SELECT ""Id"" FROM (
                        SELECT ""Id"", ROW_NUMBER() OVER (PARTITION BY ""ExplorerProfileId"", ""Source"", ""ReferenceId"" ORDER BY ""Id"") AS rn
                        FROM gamification.""XpTransactions""
                    ) sub WHERE rn = 1
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
