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
            // No-op: IX_XpTransactions_ExplorerProfileId_Source_ReferenceId is already
            // created by 20260630080921_AddVendorBranches. This migration was authored
            // independently on master before the two branches were merged and would
            // otherwise try to create the same index twice.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
