using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiTips.Api.Migrations
{
    /// <inheritdoc />
    public partial class Editorder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Invoice_InvoiceId",
                schema: "data",
                table: "Order");

            migrationBuilder.DropIndex(
                name: "IX_Order_InvoiceId",
                schema: "data",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                schema: "data",
                table: "Order");

            migrationBuilder.AddColumn<Guid>(
                name: "AccessToken",
                schema: "data",
                table: "Order",
                type: "uuid",
                nullable: true,
                comment: "Токен для получения подсказок");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessToken",
                schema: "data",
                table: "Order");

            migrationBuilder.AddColumn<Guid>(
                name: "InvoiceId",
                schema: "data",
                table: "Order",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Order_InvoiceId",
                schema: "data",
                table: "Order",
                column: "InvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Invoice_InvoiceId",
                schema: "data",
                table: "Order",
                column: "InvoiceId",
                principalSchema: "data",
                principalTable: "Invoice",
                principalColumn: "Id");
        }
    }
}
