using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlarganShipping.Migrations
{
    /// <inheritdoc />
    public partial class EditInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CustomsAmount",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomsAmount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "Invoices");
        }
    }
}
