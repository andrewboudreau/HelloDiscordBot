using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscordBotHost.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class NullSnapshotUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ContentSnapshotUrl",
                table: "ContentInspections",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ContentSnapshotUrl",
                table: "ContentInspections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
