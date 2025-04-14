using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class LotModelupdateRes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LotId1",
                table: "Reservation",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_LotId1",
                table: "Reservation",
                column: "LotId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_Lot_LotId1",
                table: "Reservation",
                column: "LotId1",
                principalTable: "Lot",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_Lot_LotId1",
                table: "Reservation");

            migrationBuilder.DropIndex(
                name: "IX_Reservation_LotId1",
                table: "Reservation");

            migrationBuilder.DropColumn(
                name: "LotId1",
                table: "Reservation");
        }
    }
}
