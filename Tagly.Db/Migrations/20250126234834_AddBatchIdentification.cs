using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tagly.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddBatchIdentification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BatchId",
                table: "Photos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BatchNumber",
                table: "Photos",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "BatchNumber",
                table: "Photos");
        }
    }
}
