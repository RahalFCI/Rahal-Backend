using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMedia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCommentIndexesOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Comments_ParentCommentId",
                schema: "socialmedia",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_PostId_ParentCommentId",
                schema: "socialmedia",
                table: "Comments");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ParentCommentId_CreatedAt",
                schema: "socialmedia",
                table: "Comments",
                columns: new[] { "ParentCommentId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_PostId_ParentCommentId_CreatedAt",
                schema: "socialmedia",
                table: "Comments",
                columns: new[] { "PostId", "ParentCommentId", "CreatedAt" },
                descending: new[] { false, false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Comments_ParentCommentId_CreatedAt",
                schema: "socialmedia",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_PostId_ParentCommentId_CreatedAt",
                schema: "socialmedia",
                table: "Comments");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ParentCommentId",
                schema: "socialmedia",
                table: "Comments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_PostId_ParentCommentId",
                schema: "socialmedia",
                table: "Comments",
                columns: new[] { "PostId", "ParentCommentId" });
        }
    }
}
