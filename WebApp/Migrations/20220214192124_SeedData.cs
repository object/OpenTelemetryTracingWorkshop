using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Migrations
{
    public partial class SeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "words",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Word = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_words", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "words",
                columns: new[] { "Id", "Word" },
                values: new object[,]
                {
                    { 1, "quick" },
                    { 2, "brown" },
                    { 3, "fox" },
                    { 4, "jumped" },
                    { 5, "over" },
                    { 6, "the" },
                    { 7, "lazy" },
                    { 8, "dog" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "words");
        }
    }
}
