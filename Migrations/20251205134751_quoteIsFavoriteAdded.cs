using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace booksBackend.Migrations
{
    /// <inheritdoc />
    public partial class quoteIsFavoriteAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isFavorite",
                table: "Quotes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isFavorite",
                table: "Quotes");
        }
    }
}
