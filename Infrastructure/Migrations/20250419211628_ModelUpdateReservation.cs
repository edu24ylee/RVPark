using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModelUpdateReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OutstandingBalance",
                table: "Reservation",
                newName: "TaxTotal");

            migrationBuilder.AddColumn<decimal>(
                name: "BaseTotal",
                table: "Reservation",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ManualFeeTotal",
                table: "Reservation",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseTotal",
                table: "Reservation");

            migrationBuilder.DropColumn(
                name: "ManualFeeTotal",
                table: "Reservation");

            migrationBuilder.RenameColumn(
                name: "TaxTotal",
                table: "Reservation",
                newName: "OutstandingBalance");
        }
    }
}
