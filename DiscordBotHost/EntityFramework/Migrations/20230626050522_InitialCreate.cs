using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscordBotHost.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentMonitorRequests",
                columns: table => new
                {
                    UrlContentMonitorRequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Selectors = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Interval = table.Column<long>(type: "bigint", nullable: false),
                    Repeat = table.Column<int>(type: "int", nullable: false),
                    RunUntil = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DifferenceThreshold = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DiscordUserId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentMonitorRequests", x => x.UrlContentMonitorRequestId);
                });

            migrationBuilder.CreateTable(
                name: "Opportunities",
                columns: table => new
                {
                    OpportunityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Company = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShowName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JobName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuditionDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AuditionEndDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Opportunities", x => x.OpportunityId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiscordUserId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    FirebaseId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LinksChannelId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContentInspections",
                columns: table => new
                {
                    UrlContentInspectionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UrlContentMonitorRequestId = table.Column<int>(type: "int", nullable: false),
                    ThresholdExceeded = table.Column<bool>(type: "bit", nullable: false),
                    Differences = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentInspections", x => x.UrlContentInspectionId);
                    table.ForeignKey(
                        name: "FK_ContentInspections_ContentMonitorRequests_UrlContentMonitorRequestId",
                        column: x => x.UrlContentMonitorRequestId,
                        principalTable: "ContentMonitorRequests",
                        principalColumn: "UrlContentMonitorRequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentInspections_UrlContentMonitorRequestId",
                table: "ContentInspections",
                column: "UrlContentMonitorRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentInspections");

            migrationBuilder.DropTable(
                name: "Opportunities");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "ContentMonitorRequests");
        }
    }
}
