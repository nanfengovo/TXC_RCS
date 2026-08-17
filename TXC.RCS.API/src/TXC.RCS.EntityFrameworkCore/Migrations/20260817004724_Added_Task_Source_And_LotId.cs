using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TXC.RCS.Migrations
{
    /// <inheritdoc />
    public partial class Added_Task_Source_And_LotId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ContainerId",
                schema: "_RCS",
                table: "TXC_Tasks",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LotId",
                schema: "_RCS",
                table: "TXC_Tasks",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Source",
                schema: "_RCS",
                table: "TXC_Tasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TXC_Tasks_Source",
                schema: "_RCS",
                table: "TXC_Tasks",
                column: "Source");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TXC_Tasks_Source",
                schema: "_RCS",
                table: "TXC_Tasks");

            migrationBuilder.DropColumn(
                name: "LotId",
                schema: "_RCS",
                table: "TXC_Tasks");

            migrationBuilder.DropColumn(
                name: "Source",
                schema: "_RCS",
                table: "TXC_Tasks");

            migrationBuilder.AlterColumn<string>(
                name: "ContainerId",
                schema: "_RCS",
                table: "TXC_Tasks",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);
        }
    }
}
