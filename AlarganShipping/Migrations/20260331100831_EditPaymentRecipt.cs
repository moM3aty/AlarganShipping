using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlarganShipping.Migrations
{
    /// <inheritdoc />
    public partial class EditPaymentRecipt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CarId",
                table: "PaymentReceipts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceipts_CarId",
                table: "PaymentReceipts",
                column: "CarId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentReceipts_Cars_CarId",
                table: "PaymentReceipts",
                column: "CarId",
                principalTable: "Cars",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentReceipts_Cars_CarId",
                table: "PaymentReceipts");

            migrationBuilder.DropIndex(
                name: "IX_PaymentReceipts_CarId",
                table: "PaymentReceipts");

            migrationBuilder.DropColumn(
                name: "CarId",
                table: "PaymentReceipts");
        }
    }
}
