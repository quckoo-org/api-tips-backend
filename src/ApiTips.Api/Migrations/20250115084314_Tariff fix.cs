using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiTips.Api.Migrations
{
    /// <inheritdoc />
    public partial class Tarifffix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipPrice",
                schema: "data",
                table: "Tariff");

            migrationBuilder.DropColumn(
                name: "TotalTipsCount",
                schema: "data",
                table: "Tariff");

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

            migrationBuilder.AddColumn<long>(
                name: "CreateById",
                schema: "data",
                table: "Tariff",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDateTime",
                schema: "data",
                table: "Tariff",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                comment: "Дата создания записи в БД по UTC");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                schema: "data",
                table: "Tariff",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "",
                comment: "Валюта тарифа, ISO 4217");

            migrationBuilder.CreateIndex(
                name: "IX_Tariff_CreateById",
                schema: "data",
                table: "Tariff",
                column: "CreateById");

            migrationBuilder.AddForeignKey(
                name: "FK_Tariff_User_CreateById",
                schema: "data",
                table: "Tariff",
                column: "CreateById",
                principalSchema: "system",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tariff_User_CreateById",
                schema: "data",
                table: "Tariff");

            migrationBuilder.DropIndex(
                name: "IX_Tariff_CreateById",
                schema: "data",
                table: "Tariff");

            migrationBuilder.DropColumn(
                name: "ArchiveDateTime",
                schema: "data",
                table: "Tariff");

            migrationBuilder.DropColumn(
                name: "CreateById",
                schema: "data",
                table: "Tariff");

            migrationBuilder.DropColumn(
                name: "CreateDateTime",
                schema: "data",
                table: "Tariff");

            migrationBuilder.DropColumn(
                name: "Currency",
                schema: "data",
                table: "Tariff");

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

            migrationBuilder.AddColumn<decimal>(
                name: "TipPrice",
                schema: "data",
                table: "Tariff",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                comment: "Стоимость одной подсказки");

            migrationBuilder.AddColumn<int>(
                name: "TotalTipsCount",
                schema: "data",
                table: "Tariff",
                type: "integer",
                nullable: true,
                comment: "Общее количество подсказок");
        }
    }
}
