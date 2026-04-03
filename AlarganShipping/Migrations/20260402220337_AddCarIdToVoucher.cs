using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlarganShipping.Migrations
{
    /// <inheritdoc />
    public partial class AddCarIdToVoucher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CarId",
                table: "PaymentVouchers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentVouchers_CarId",
                table: "PaymentVouchers",
                column: "CarId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentVouchers_Cars_CarId",
                table: "PaymentVouchers",
                column: "CarId",
                principalTable: "Cars",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentVouchers_Cars_CarId",
                table: "PaymentVouchers");

            migrationBuilder.DropIndex(
                name: "IX_PaymentVouchers_CarId",
                table: "PaymentVouchers");

            migrationBuilder.DropColumn(
                name: "CarId",
                table: "PaymentVouchers");
        }
    }
}
