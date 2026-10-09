using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ambio.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddCompanies : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Companies",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                Kind = table.Column<string>(type: "TEXT", nullable: false),
                Website = table.Column<string>(type: "TEXT", nullable: true),
                Industry = table.Column<string>(type: "TEXT", nullable: true),
                Size = table.Column<string>(type: "TEXT", nullable: true),
                Location = table.Column<string>(type: "TEXT", nullable: true),
                LinkedInUrl = table.Column<string>(type: "TEXT", nullable: true),
                Description = table.Column<string>(type: "TEXT", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                ArchivedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Companies", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Companies_Name",
            table: "Companies",
            column: "Name",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Companies");
    }
}
