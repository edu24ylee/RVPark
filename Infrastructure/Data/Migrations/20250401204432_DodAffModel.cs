using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DodAffModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationDate",
                table: "ReservationReports");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "ReservationReports");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "ReservationReports");

            migrationBuilder.DropColumn(
                name: "OverrideReason",
                table: "ReservationReports");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ReservationReports");

            migrationBuilder.DropColumn(
                name: "TotalPaid",
                table: "ReservationReports");

            migrationBuilder.CreateTable(
                name: "DodAffiliation",
                columns: table => new
                {
                    DodAffiliationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Branch = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rank = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GuestID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DodAffiliation", x => x.DodAffiliationId);
                    table.ForeignKey(
                        name: "FK_DodAffiliation_Guest_GuestID",
                        column: x => x.GuestID,
                        principalTable: "Guest",
                        principalColumn: "GuestID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DodAffiliation_GuestID",
                table: "DodAffiliation",
                column: "GuestID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DodAffiliation");

            migrationBuilder.AddColumn<DateTime>(
                name: "CancellationDate",
                table: "ReservationReports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "ReservationReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "ReservationReports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OverrideReason",
                table: "ReservationReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ReservationReports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPaid",
                table: "ReservationReports",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
