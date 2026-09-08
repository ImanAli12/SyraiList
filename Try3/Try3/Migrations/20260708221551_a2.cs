using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Try3.Migrations
{
    /// <inheritdoc />
    public partial class a2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertyImages_PropertiesSample_PropertySampleId",
                table: "PropertyImages");

            migrationBuilder.DropTable(
                name: "PropertiesSample");

            migrationBuilder.DropTable(
                name: "SimilarProperties");

            migrationBuilder.DropIndex(
                name: "IX_PropertyImages_PropertySampleId",
                table: "PropertyImages");

            migrationBuilder.DropColumn(
                name: "PropertySampleId",
                table: "PropertyImages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                    AdvertiserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    PropertyTypeId = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AdvertiserPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Area = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AvailableFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Bathrooms = table.Column<byte>(type: "tinyint", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetailedLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Floor = table.Column<short>(type: "smallint", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    Neighborhood = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceCurrency = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Rooms = table.Column<byte>(type: "tinyint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ViewsCount = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "SimilarProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    property_code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    rank_order = table.Column<int>(type: "int", nullable: false),
                    similar_property_code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    similarity_score = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimilarProperties", x => x.Id);
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
    }
}
