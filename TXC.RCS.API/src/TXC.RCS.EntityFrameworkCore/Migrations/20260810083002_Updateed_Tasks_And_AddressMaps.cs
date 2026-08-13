using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TXC.RCS.Migrations
{
    /// <inheritdoc />
    public partial class Updateed_Tasks_And_AddressMaps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TmStorage",
                schema: "_RCS",
                table: "TXC_AddressMaps",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TmStorage",
                schema: "_RCS",
                table: "TXC_AddressMaps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
