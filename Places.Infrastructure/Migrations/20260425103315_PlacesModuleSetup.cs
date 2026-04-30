using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Places.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PlacesModuleSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "places");

            migrationBuilder.CreateTable(
                name: "PlaceCategories",
                schema: "places",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false, comment: "Detailed description of the place category"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaceCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Places",
                schema: "places",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false, comment: "Detailed description of the place"),
                    PlaceCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketPrice = table.Column<double>(type: "double precision", precision: 10, scale: 2, nullable: false, defaultValue: 0.0),
                    Latitude = table.Column<double>(type: "double precision", precision: 10, scale: 8, nullable: false, comment: "Geographic latitude coordinate"),
                    Longitude = table.Column<double>(type: "double precision", precision: 11, scale: 8, nullable: false, comment: "Geographic longitude coordinate"),
                    Address_AddressLine = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address_Government = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address_City = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address_Country = table.Column<string>(type: "text", nullable: true),
                    GeofenceRange = table.Column<int>(type: "integer", nullable: false, defaultValue: 100, comment: "Radius in meters for geofence validation"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Places", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Places_PlaceCategories_PlaceCategoryId",
                        column: x => x.PlaceCategoryId,
                        principalSchema: "places",
                        principalTable: "PlaceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CheckIns",
                schema: "places",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExplorerId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValidationStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Validation status of the check-in (Pending=0, Approved=1, Rejected=2)"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckIns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckIns_Places_PlaceId",
                        column: x => x.PlaceId,
                        principalSchema: "places",
                        principalTable: "Places",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlacePhotos",
                schema: "places",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "URL of the place photo"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlacePhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlacePhotos_Places_PlaceId",
                        column: x => x.PlaceId,
                        principalSchema: "places",
                        principalTable: "Places",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaceReviews",
                schema: "places",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExplorerId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckInId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Rating value (typically 1-5)"),
                    Comment = table.Column<string>(type: "text", nullable: false, comment: "Review comment/feedback from explorer"),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "Whether the review is verified by moderators"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaceReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaceReviews_CheckIns_CheckInId",
                        column: x => x.CheckInId,
                        principalSchema: "places",
                        principalTable: "CheckIns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaceReviews_Places_PlaceId",
                        column: x => x.PlaceId,
                        principalSchema: "places",
                        principalTable: "Places",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_ExplorerId",
                schema: "places",
                table: "CheckIns",
                column: "ExplorerId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_PlaceId",
                schema: "places",
                table: "CheckIns",
                column: "PlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_PlaceId_ValidationStatus",
                schema: "places",
                table: "CheckIns",
                columns: new[] { "PlaceId", "ValidationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_ValidationStatus",
                schema: "places",
                table: "CheckIns",
                column: "ValidationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_PlaceCategories_CreatedAt",
                schema: "places",
                table: "PlaceCategories",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PlaceCategories_IsDeleted",
                schema: "places",
                table: "PlaceCategories",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_PlaceCategories_Name_Unique",
                schema: "places",
                table: "PlaceCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlacePhotos_PlaceId",
                schema: "places",
                table: "PlacePhotos",
                column: "PlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacePhotos_Url",
                schema: "places",
                table: "PlacePhotos",
                column: "Url");

            migrationBuilder.CreateIndex(
                name: "IX_PlaceReviews_CheckInId",
                schema: "places",
                table: "PlaceReviews",
                column: "CheckInId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaceReviews_ExplorerId",
                schema: "places",
                table: "PlaceReviews",
                column: "ExplorerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaceReviews_IsVerified",
                schema: "places",
                table: "PlaceReviews",
                column: "IsVerified");

            migrationBuilder.CreateIndex(
                name: "IX_PlaceReviews_PlaceId",
                schema: "places",
                table: "PlaceReviews",
                column: "PlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaceReviews_PlaceId_Rating",
                schema: "places",
                table: "PlaceReviews",
                columns: new[] { "PlaceId", "Rating" });

            migrationBuilder.CreateIndex(
                name: "IX_PlaceReviews_Rating",
                schema: "places",
                table: "PlaceReviews",
                column: "Rating");

            migrationBuilder.CreateIndex(
                name: "IX_Places_Address_City",
                schema: "places",
                table: "Places",
                column: "Address_City");

            migrationBuilder.CreateIndex(
                name: "IX_Places_Address_Government",
                schema: "places",
                table: "Places",
                column: "Address_Government");

            migrationBuilder.CreateIndex(
                name: "IX_Places_Coordinates",
                schema: "places",
                table: "Places",
                columns: new[] { "Latitude", "Longitude" });

            migrationBuilder.CreateIndex(
                name: "IX_Places_CreatedAt",
                schema: "places",
                table: "Places",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Places_IsDeleted",
                schema: "places",
                table: "Places",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Places_Name",
                schema: "places",
                table: "Places",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Places_PlaceCategoryId",
                schema: "places",
                table: "Places",
                column: "PlaceCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlacePhotos",
                schema: "places");

            migrationBuilder.DropTable(
                name: "PlaceReviews",
                schema: "places");

            migrationBuilder.DropTable(
                name: "CheckIns",
                schema: "places");

            migrationBuilder.DropTable(
                name: "Places",
                schema: "places");

            migrationBuilder.DropTable(
                name: "PlaceCategories",
                schema: "places");
        }
    }
}
