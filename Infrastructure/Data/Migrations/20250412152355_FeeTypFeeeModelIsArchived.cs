using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    public partial class FeeTypFeeeModelIsArchived : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "FeeType",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "FeeType",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TriggerRuleJson",
                table: "FeeType",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TriggerType",
                table: "FeeType",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "AppliedDate",
                table: "Fee",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Fee",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReservationId",
                table: "Fee",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TriggerType",
                table: "Fee",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Fee_ReservationId",
                table: "Fee",
                column: "ReservationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Fee_Reservation_ReservationId",
                table: "Fee",
                column: "ReservationId",
                principalTable: "Reservation",
                principalColumn: "ReservationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fee_Reservation_ReservationId",
                table: "Fee");

            migrationBuilder.DropIndex(
                name: "IX_Fee_ReservationId",
                table: "Fee");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "FeeType");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "FeeType");

            migrationBuilder.DropColumn(
                name: "TriggerRuleJson",
                table: "FeeType");

            migrationBuilder.DropColumn(
                name: "TriggerType",
                table: "FeeType");

            migrationBuilder.DropColumn(
                name: "AppliedDate",
                table: "Fee");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Fee");

            migrationBuilder.DropColumn(
                name: "ReservationId",
                table: "Fee");

            migrationBuilder.DropColumn(
                name: "TriggerType",
                table: "Fee");
        }
    }
}
