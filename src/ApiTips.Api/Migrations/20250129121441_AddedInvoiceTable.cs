using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiTips.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddedInvoiceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InvoiceId",
                schema: "data",
                table: "Order",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Invoice",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Уникальный идентификатор счета"),
                    PayerId = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    RefNumber = table.Column<string>(type: "text", nullable: false, comment: "REF-номер заказа, по которому выставлен счет"),
                    Alias = table.Column<string>(type: "text", nullable: false, comment: "Алиас счёта, генерирующийся при создании"),
                    AmountOfRequests = table.Column<long>(type: "bigint", nullable: false, comment: "Общее количество запросов"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата создания счета"),
                    PayedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Дата оплаты счета"),
                    Description = table.Column<string>(type: "text", nullable: true, comment: "Комментарий к счету"),
                    CurrentCurrency = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoice_Order_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "data",
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Invoice_User_PayerId",
                        column: x => x.PayerId,
                        principalSchema: "system",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Объект - заказ");

            migrationBuilder.CreateIndex(
                name: "IX_Order_InvoiceId",
                schema: "data",
                table: "Order",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_OrderId",
                schema: "data",
                table: "Invoice",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_PayerId",
                schema: "data",
                table: "Invoice",
                column: "PayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Invoice_InvoiceId",
                schema: "data",
                table: "Order",
                column: "InvoiceId",
                principalSchema: "data",
                principalTable: "Invoice",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Invoice_InvoiceId",
                schema: "data",
                table: "Order");

            migrationBuilder.DropTable(
                name: "Invoice",
                schema: "data");

            migrationBuilder.DropIndex(
                name: "IX_Order_InvoiceId",
                schema: "data",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                schema: "data",
                table: "Order");
        }
    }
}
