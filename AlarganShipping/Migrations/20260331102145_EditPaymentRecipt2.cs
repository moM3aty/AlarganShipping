using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlarganShipping.Migrations
{
    /// <inheritdoc />
    public partial class EditPaymentRecipt2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "PaymentReceipts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankName",
                table: "PaymentReceipts");
        }
    }
}
