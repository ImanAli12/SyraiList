using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Try3.Migrations
{
    /// <inheritdoc />
    public partial class Py : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SimilarProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SimilarPropertyCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SimilarityScore = table.Column<double>(type: "float", nullable: false),
                    RankOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimilarProperties", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SimilarProperties");
        }
    }
}
