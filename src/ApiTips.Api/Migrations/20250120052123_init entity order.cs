using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ApiTips.Api.Migrations
{
    /// <inheritdoc />
    public partial class initentityorder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Order",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "Уникальный идентификатор заказа")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата создания заказа"),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "Статус заказа"),
                    PaymentDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Дата оплаты заказа"),
                    TariffId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Order_Tariff_TariffId",
                        column: x => x.TariffId,
                        principalSchema: "data",
                        principalTable: "Tariff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Order_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "system",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Объект - заказ");

            migrationBuilder.CreateIndex(
                name: "IX_Order_TariffId",
                schema: "data",
                table: "Order",
                column: "TariffId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_UserId",
                schema: "data",
                table: "Order",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Order",
                schema: "data");
        }
    }
}
