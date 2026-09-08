using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Try3.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Neighborhoods_NeighborhoodId",
                table: "Properties");

            migrationBuilder.AlterColumn<int>(
                name: "NeighborhoodId",
                table: "Properties",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "AdvertiserPhone",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AvailableFrom",
                table: "Properties",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DetailedLocation",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Neighborhood",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Features",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IconClass = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Features", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeatureProperty",
                columns: table => new
                {
                    FeaturesId = table.Column<int>(type: "int", nullable: false),
                    PropertiesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureProperty", x => new { x.FeaturesId, x.PropertiesId });
                    table.ForeignKey(
                        name: "FK_FeatureProperty_Features_FeaturesId",
                        column: x => x.FeaturesId,
                        principalTable: "Features",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeatureProperty_Properties_PropertiesId",
                        column: x => x.PropertiesId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeatureProperty_PropertiesId",
                table: "FeatureProperty",
                column: "PropertiesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Neighborhoods_NeighborhoodId",
                table: "Properties",
                column: "NeighborhoodId",
                principalTable: "Neighborhoods",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Neighborhoods_NeighborhoodId",
                table: "Properties");

            migrationBuilder.DropTable(
                name: "FeatureProperty");

            migrationBuilder.DropTable(
                name: "Features");

            migrationBuilder.DropColumn(
                name: "AdvertiserPhone",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "AvailableFrom",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "DetailedLocation",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Neighborhood",
                table: "Properties");

            migrationBuilder.AlterColumn<int>(
                name: "NeighborhoodId",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Neighborhoods_NeighborhoodId",
                table: "Properties",
                column: "NeighborhoodId",
                principalTable: "Neighborhoods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
