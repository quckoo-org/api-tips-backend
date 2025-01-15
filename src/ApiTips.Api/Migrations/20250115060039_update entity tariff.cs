using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiTips.Api.Migrations
{
    /// <inheritdoc />
    public partial class updateentitytariff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPrice",
                schema: "data",
                table: "Tariff",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                comment: "Общая стоимость тарифа, вводится менеджером",
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true,
                oldComment: "Общая стоимость");

            migrationBuilder.AlterColumn<long>(
                name: "PaidTipsCount",
                schema: "data",
                table: "Tariff",
                type: "bigint",
                nullable: true,
                comment: "Количество оплаченных подсказок",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Количество оплаченных подсказок");

            migrationBuilder.AlterColumn<long>(
                name: "FreeTipsCount",
                schema: "data",
                table: "Tariff",
                type: "bigint",
                nullable: true,
                comment: "Количество бесплатных подсказок",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Количество бесплатных подсказок");

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchiveDateTime",
                schema: "data",
                table: "Tariff",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата архивации тарифа");

            migrationBuilder.AlterColumn<long>(
                name: "TotalTipsCount",
                schema: "data",
                table: "Tariff",
                type: "bigint",
                rowVersion: true,
                nullable: false,
                computedColumnSql: "ISNULL([FreeTipsCount], 0) + ISNULL([PaidTipsCount], 0)",
                stored: false,
                comment: "Общее количество подсказок, вычисляемое поле",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Общее количество подсказок");

            migrationBuilder.AlterColumn<decimal>(
                name: "TipPrice",
                schema: "data",
                table: "Tariff",
                type: "numeric",
                rowVersion: true,
                nullable: false,
                computedColumnSql: "CASE WHEN ISNULL(NULLIF([PaidTipsCount], 0), 0) > 0 THEN [TotalPrice] / [PaidTipsCount] ELSE 0",
                stored: false,
                comment: "Стоимость одной подсказки, вычисляемое поле",
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldComment: "Стоимость одной подсказки");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchiveDateTime",
                schema: "data",
                table: "Tariff");

            migrationBuilder.AlterColumn<int>(
                name: "TotalTipsCount",
                schema: "data",
                table: "Tariff",
                type: "integer",
                nullable: true,
                comment: "Общее количество подсказок",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldRowVersion: true,
                oldComputedColumnSql: "ISNULL([FreeTipsCount], 0) + ISNULL([PaidTipsCount], 0)",
                oldComment: "Общее количество подсказок, вычисляемое поле");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPrice",
                schema: "data",
                table: "Tariff",
                type: "numeric",
                nullable: true,
                comment: "Общая стоимость",
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldComment: "Общая стоимость тарифа, вводится менеджером");

            migrationBuilder.AlterColumn<decimal>(
                name: "TipPrice",
                schema: "data",
                table: "Tariff",
                type: "numeric",
                nullable: false,
                comment: "Стоимость одной подсказки",
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldRowVersion: true,
                oldComputedColumnSql: "CASE WHEN ISNULL(NULLIF([PaidTipsCount], 0), 0) > 0 THEN [TotalPrice] / [PaidTipsCount] ELSE 0",
                oldComment: "Стоимость одной подсказки, вычисляемое поле");

            migrationBuilder.AlterColumn<int>(
                name: "PaidTipsCount",
                schema: "data",
                table: "Tariff",
                type: "integer",
                nullable: true,
                comment: "Количество оплаченных подсказок",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true,
                oldComment: "Количество оплаченных подсказок");

            migrationBuilder.AlterColumn<int>(
                name: "FreeTipsCount",
                schema: "data",
                table: "Tariff",
                type: "integer",
                nullable: true,
                comment: "Количество бесплатных подсказок",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true,
                oldComment: "Количество бесплатных подсказок");
        }
    }
}
