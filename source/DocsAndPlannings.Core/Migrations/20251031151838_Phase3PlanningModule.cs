using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocsAndPlannings.Core.Migrations;

/// <inheritdoc />
public partial class Phase3PlanningModule : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "DocumentAttachments",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                DocumentId = table.Column<int>(type: "INTEGER", nullable: false),
                FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                StoredFileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                ContentType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                FileSizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                UploadedById = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DocumentAttachments", x => x.Id);
                table.ForeignKey(
                    name: "FK_DocumentAttachments_Documents_DocumentId",
                    column: x => x.DocumentId,
                    principalTable: "Documents",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_DocumentAttachments_Users_UploadedById",
                    column: x => x.UploadedById,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_DocumentAttachments_DocumentId",
            table: "DocumentAttachments",
            column: "DocumentId");

        migrationBuilder.CreateIndex(
            name: "IX_DocumentAttachments_UploadedById",
            table: "DocumentAttachments",
            column: "UploadedById");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "DocumentAttachments");
    }
}
