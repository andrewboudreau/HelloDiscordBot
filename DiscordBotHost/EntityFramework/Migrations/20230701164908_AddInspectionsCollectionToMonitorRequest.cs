using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscordBotHost.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddInspectionsCollectionToMonitorRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Selectors",
                table: "MonitorContentRequests",
                newName: "Selector");

            migrationBuilder.AddColumn<string>(
                name: "ContentSnapshotUrl",
                table: "ContentInspections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentSnapshotUrl",
                table: "ContentInspections");

            migrationBuilder.RenameColumn(
                name: "Selector",
                table: "MonitorContentRequests",
                newName: "Selectors");
        }
    }
}
