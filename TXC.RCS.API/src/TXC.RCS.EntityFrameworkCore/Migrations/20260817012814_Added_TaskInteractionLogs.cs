using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TXC.RCS.Migrations
{
    /// <inheritdoc />
    public partial class Added_TaskInteractionLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TXC_TaskInteractionLogs",
                schema: "_RCS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    EventName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Leg = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    DetailJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TXC_TaskInteractionLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TXC_TaskInteractionLogs_TaskId",
                schema: "_RCS",
                table: "TXC_TaskInteractionLogs",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TXC_TaskInteractionLogs_TaskId_CreationTime",
                schema: "_RCS",
                table: "TXC_TaskInteractionLogs",
                columns: new[] { "TaskId", "CreationTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TXC_TaskInteractionLogs",
                schema: "_RCS");
        }
    }
}
