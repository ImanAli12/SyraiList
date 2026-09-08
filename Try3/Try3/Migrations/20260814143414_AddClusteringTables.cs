using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Try3.Migrations
{
    /// <inheritdoc />
    public partial class AddClusteringTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AvgNeighborhoodEnc",
                table: "ClusterCenters",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "AvgPropertyTypeId",
                table: "ClusterCenters",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "NeighborhoodEncodings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    NeighborhoodName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EncodedValue = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NeighborhoodEncodings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NeighborhoodEncodings_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NormalizationParams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    FeatureName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MinValue = table.Column<double>(type: "float", nullable: false),
                    MaxValue = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormalizationParams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NormalizationParams_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NeighborhoodEncodings_CityId",
                table: "NeighborhoodEncodings",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_NormalizationParams_CityId",
                table: "NormalizationParams",
                column: "CityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NeighborhoodEncodings");

            migrationBuilder.DropTable(
                name: "NormalizationParams");

            migrationBuilder.DropColumn(
                name: "AvgNeighborhoodEnc",
                table: "ClusterCenters");

            migrationBuilder.DropColumn(
                name: "AvgPropertyTypeId",
                table: "ClusterCenters");
        }
    }
}
