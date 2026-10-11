using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ambio.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddCompanySlug : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Slug",
            table: "Companies",
            type: "TEXT",
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateIndex(
            name: "IX_Companies_Slug",
            table: "Companies",
            column: "Slug",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Companies_Slug",
            table: "Companies");

        migrationBuilder.DropColumn(
            name: "Slug",
            table: "Companies");
    }
}
