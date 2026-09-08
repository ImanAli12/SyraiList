using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Try3.Migrations
{
    /// <inheritdoc />
    public partial class AddClusterCenters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClusterCenters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    ClusterId = table.Column<int>(type: "int", nullable: false),
                    AvgPrice = table.Column<double>(type: "float", nullable: false),
                    AvgArea = table.Column<double>(type: "float", nullable: false),
                    AvgRooms = table.Column<double>(type: "float", nullable: false),
                    AvgBathrooms = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClusterCenters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClusterCenters_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClusterCenters_CityId",
                table: "ClusterCenters",
                column: "CityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClusterCenters");
        }
    }
}
