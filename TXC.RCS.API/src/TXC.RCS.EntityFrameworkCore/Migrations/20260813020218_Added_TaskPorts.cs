using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TXC.RCS.Migrations
{
    /// <inheritdoc />
    public partial class Added_TaskPorts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ToAddress",
                schema: "_RCS",
                table: "TXC_Tasks",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MiddleAddress",
                schema: "_RCS",
                table: "TXC_Tasks",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FromPort",
                schema: "_RCS",
                table: "TXC_Tasks",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MiddlePort",
                schema: "_RCS",
                table: "TXC_Tasks",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToPort",
                schema: "_RCS",
                table: "TXC_Tasks",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromPort",
                schema: "_RCS",
                table: "TXC_Tasks");

            migrationBuilder.DropColumn(
                name: "MiddlePort",
                schema: "_RCS",
                table: "TXC_Tasks");

            migrationBuilder.DropColumn(
                name: "ToPort",
                schema: "_RCS",
                table: "TXC_Tasks");

            migrationBuilder.AlterColumn<string>(
                name: "ToAddress",
                schema: "_RCS",
                table: "TXC_Tasks",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MiddleAddress",
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
