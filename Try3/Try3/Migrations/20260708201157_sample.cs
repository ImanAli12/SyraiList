using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Try3.Migrations
{
    /// <inheritdoc />
    public partial class sample : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SimilarityScore",
                table: "SimilarProperties",
                newName: "similarity_score");

            migrationBuilder.RenameColumn(
                name: "SimilarPropertyCode",
                table: "SimilarProperties",
                newName: "similar_property_code");

            migrationBuilder.RenameColumn(
                name: "RankOrder",
                table: "SimilarProperties",
                newName: "rank_order");

            migrationBuilder.RenameColumn(
                name: "PropertyCode",
                table: "SimilarProperties",
                newName: "property_code");

            migrationBuilder.AlterColumn<string>(
                name: "similar_property_code",
                table: "SimilarProperties",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "property_code",
                table: "SimilarProperties",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "PropertySampleId",
                table: "PropertyImages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PropertiesSample",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceCurrency = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Area = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rooms = table.Column<byte>(type: "tinyint", nullable: false),
                    Bathrooms = table.Column<byte>(type: "tinyint", nullable: false),
                    Floor = table.Column<short>(type: "smallint", nullable: true),
                    PropertyTypeId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    Neighborhood = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetailedLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AvailableFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdvertiserPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdvertiserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ViewsCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertiesSample", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertiesSample_AspNetUsers_AdvertiserId",
                        column: x => x.AdvertiserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertiesSample_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertiesSample_PropertyTypes_PropertyTypeId",
                        column: x => x.PropertyTypeId,
                        principalTable: "PropertyTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyImages_PropertySampleId",
                table: "PropertyImages",
                column: "PropertySampleId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertiesSample_AdvertiserId",
                table: "PropertiesSample",
                column: "AdvertiserId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertiesSample_CityId",
                table: "PropertiesSample",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertiesSample_PropertyTypeId",
                table: "PropertiesSample",
                column: "PropertyTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyImages_PropertiesSample_PropertySampleId",
                table: "PropertyImages",
                column: "PropertySampleId",
                principalTable: "PropertiesSample",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertyImages_PropertiesSample_PropertySampleId",
                table: "PropertyImages");

            migrationBuilder.DropTable(
                name: "PropertiesSample");

            migrationBuilder.DropIndex(
                name: "IX_PropertyImages_PropertySampleId",
                table: "PropertyImages");

            migrationBuilder.DropColumn(
                name: "PropertySampleId",
                table: "PropertyImages");

            migrationBuilder.RenameColumn(
                name: "similarity_score",
                table: "SimilarProperties",
                newName: "SimilarityScore");

            migrationBuilder.RenameColumn(
                name: "similar_property_code",
                table: "SimilarProperties",
                newName: "SimilarPropertyCode");

            migrationBuilder.RenameColumn(
                name: "rank_order",
                table: "SimilarProperties",
                newName: "RankOrder");

            migrationBuilder.RenameColumn(
                name: "property_code",
                table: "SimilarProperties",
                newName: "PropertyCode");

            migrationBuilder.AlterColumn<string>(
                name: "SimilarPropertyCode",
                table: "SimilarProperties",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PropertyCode",
                table: "SimilarProperties",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
