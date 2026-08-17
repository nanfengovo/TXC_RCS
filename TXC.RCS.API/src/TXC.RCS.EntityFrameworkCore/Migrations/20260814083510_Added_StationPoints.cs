using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TXC.RCS.Migrations
{
    /// <inheritdoc />
    public partial class Added_StationPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TXC_StationPoints",
                schema: "_RCS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Port = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    MasterValuesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TXC_StationPoints", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TXC_StationPoints_AddressCode_Port",
                schema: "_RCS",
                table: "TXC_StationPoints",
                columns: new[] { "AddressCode", "Port" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TXC_StationPoints",
                schema: "_RCS");
        }
    }
}
