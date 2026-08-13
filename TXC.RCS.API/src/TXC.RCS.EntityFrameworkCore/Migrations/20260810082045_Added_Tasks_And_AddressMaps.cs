using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TXC.RCS.Migrations
{
    /// <inheritdoc />
    public partial class Added_Tasks_And_AddressMaps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "_RCS");

            migrationBuilder.CreateTable(
                name: "TXC_AddressMaps",
                schema: "_RCS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TmTarget = table.Column<int>(type: "int", nullable: false),
                    TmStorage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TXC_AddressMaps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TXC_Tasks",
                schema: "_RCS",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContainerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MiddleAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FetchCount = table.Column<int>(type: "int", nullable: true),
                    PutCount = table.Column<int>(type: "int", nullable: true),
                    FetchMaterialCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PutMaterialCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FetchEquipmentCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PutEquipmentCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FetchTaskSerial = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PutTaskSerial = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    AgvSerial = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromTmTarget = table.Column<int>(type: "int", nullable: false),
                    FromTmStorage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToTmTarget = table.Column<int>(type: "int", nullable: false),
                    ToTmStorage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FetchOptionCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PutOptionCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TaskLifecycleStatus = table.Column<int>(type: "int", nullable: false),
                    TemplateCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TemplateVersion = table.Column<int>(type: "int", nullable: false),
                    StepIndex = table.Column<int>(type: "int", nullable: false),
                    WaitingEvent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActiveLeg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StepsSnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RuntimeVarsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TXC_Tasks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TXC_AddressMaps_AddressCode",
                schema: "_RCS",
                table: "TXC_AddressMaps",
                column: "AddressCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TXC_Tasks_FetchTaskSerial",
                schema: "_RCS",
                table: "TXC_Tasks",
                column: "FetchTaskSerial");

            migrationBuilder.CreateIndex(
                name: "IX_TXC_Tasks_PutTaskSerial",
                schema: "_RCS",
                table: "TXC_Tasks",
                column: "PutTaskSerial");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TXC_AddressMaps",
                schema: "_RCS");

            migrationBuilder.DropTable(
                name: "TXC_Tasks",
                schema: "_RCS");
        }
    }
}
