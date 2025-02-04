using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ApiTips.Api.Migrations
{
    /// <inheritdoc />
    public partial class initbalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Balance",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "Уникальный идентификатор баланса")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FreeTipsCount = table.Column<long>(type: "bigint", nullable: false, comment: "Количество бесплатных подсказок"),
                    PaidTipsCount = table.Column<long>(type: "bigint", nullable: false, comment: "Количество оплаченных подсказок"),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Balance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Balance_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "system",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Объект - баланс");

            migrationBuilder.CreateTable(
                name: "BalanceHistory",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "Уникальный идентификатор записи истории изменения баланса")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FreeTipsCountChangedTo = table.Column<long>(type: "bigint", nullable: true, comment: "Величина изменения количества бесплатных подсказок"),
                    PaidTipsCountChangedTo = table.Column<long>(type: "bigint", nullable: true, comment: "Величина изменения количества оплаченных подсказок"),
                    OperationType = table.Column<int>(type: "integer", nullable: false, comment: "Тип операции (пополнение/списание)"),
                    ReasonDescription = table.Column<string>(type: "text", nullable: false, comment: "Описание причины изменения баланса (пример: покупка/списание/промо)"),
                    OperationDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()", comment: "Дата совершения операции"),
                    TotalTipsBalance = table.Column<long>(type: "bigint", nullable: false, comment: "Количество подсказок на балансе после выполнения операции"),
                    BalanceId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BalanceHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BalanceHistory_Balance_BalanceId",
                        column: x => x.BalanceId,
                        principalSchema: "data",
                        principalTable: "Balance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Объект - изменение баланса");

            migrationBuilder.CreateIndex(
                name: "IX_Balance_UserId",
                schema: "data",
                table: "Balance",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BalanceHistory_BalanceId",
                schema: "data",
                table: "BalanceHistory",
                column: "BalanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BalanceHistory",
                schema: "data");

            migrationBuilder.DropTable(
                name: "Balance",
                schema: "data");
        }
    }
}
