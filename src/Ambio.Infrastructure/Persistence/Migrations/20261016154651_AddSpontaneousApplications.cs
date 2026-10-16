using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ambio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSpontaneousApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "JobApplications",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetPosition",
                table: "JobApplications",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_CompanyId",
                table: "JobApplications",
                column: "CompanyId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_JobApplications_Kind",
                table: "JobApplications",
                sql: "(\"Kind\" = 'Offer' AND \"JobOfferId\" IS NOT NULL\n    AND \"CompanyId\" IS NULL AND \"TargetPosition\" IS NULL)\nOR (\"Kind\" = 'Spontaneous' AND \"CompanyId\" IS NOT NULL\n    AND \"TargetPosition\" IS NOT NULL AND \"JobOfferId\" IS NULL AND \"AppliedViaPostingId\" IS NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplications_Companies_CompanyId",
                table: "JobApplications",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobApplications_Companies_CompanyId",
                table: "JobApplications");

            migrationBuilder.DropIndex(
                name: "IX_JobApplications_CompanyId",
                table: "JobApplications");

            migrationBuilder.DropCheckConstraint(
                name: "CK_JobApplications_Kind",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "TargetPosition",
                table: "JobApplications");
        }
    }
}
