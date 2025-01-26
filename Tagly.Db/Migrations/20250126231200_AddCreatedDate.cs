using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tagly.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Photos",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Created",
                table: "Photos");
        }
    }
}
