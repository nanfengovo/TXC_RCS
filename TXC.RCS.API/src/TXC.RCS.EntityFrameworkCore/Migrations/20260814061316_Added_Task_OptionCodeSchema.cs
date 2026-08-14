using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TXC.RCS.Migrations
{
    /// <inheritdoc />
    public partial class Added_Task_OptionCodeSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OptionCodeSchemaCode",
                schema: "_RCS",
                table: "TXC_Tasks",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "OptionCodeSchemaVersion",
                schema: "_RCS",
                table: "TXC_Tasks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OptionCodeSchemaCode",
                schema: "_RCS",
                table: "TXC_Tasks");

            migrationBuilder.DropColumn(
                name: "OptionCodeSchemaVersion",
                schema: "_RCS",
                table: "TXC_Tasks");
        }
    }
}
