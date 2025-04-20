using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class feetotal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultFeeTotal",
                table: "FeeType");

            migrationBuilder.AddColumn<string>(
                name: "FeeLabel",
                table: "Fee",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeeLabel",
                table: "Fee");

            migrationBuilder.AddColumn<decimal>(
                name: "DefaultFeeTotal",
                table: "FeeType",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
