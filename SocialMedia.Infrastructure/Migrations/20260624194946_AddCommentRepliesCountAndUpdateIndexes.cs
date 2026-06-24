using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMedia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentRepliesCountAndUpdateIndexes : Migration
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

            migrationBuilder.AddColumn<int>(
                name: "RepliesCount",
                schema: "socialmedia",
                table: "Comments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Comments_ParentCommentId",
                schema: "socialmedia",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_PostId_ParentCommentId",
                schema: "socialmedia",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "RepliesCount",
                schema: "socialmedia",
                table: "Comments");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ParentCommentId",
                schema: "socialmedia",
                table: "Comments",
                column: "ParentCommentId",
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_PostId_ParentCommentId",
                schema: "socialmedia",
                table: "Comments",
                columns: new[] { "PostId", "ParentCommentId" },
                filter: "\"IsDeleted\" = false");
        }
    }
}
