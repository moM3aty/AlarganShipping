using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlarganShipping.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInvoiceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsPaid",
                table: "Invoices",
                newName: "IsShippingOnly");

            migrationBuilder.AddColumn<decimal>(
                name: "AmountPaid",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "StorageFees",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SellingPrice",
                table: "Cars",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountPaid",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "StorageFees",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SellingPrice",
                table: "Cars");

            migrationBuilder.RenameColumn(
                name: "IsShippingOnly",
                table: "Invoices",
                newName: "IsPaid");
        }
    }
}
