using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ApiTips.Api.Migrations
{
    /// <inheritdoc />
    public partial class initentitytariff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "data");

            migrationBuilder.CreateTable(
                name: "Tariff",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "Уникальный идентификатор тарифа")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Уникальное название тарифа"),
                    TipPrice = table.Column<decimal>(type: "numeric", nullable: false, comment: "Стоимость одной подсказки"),
                    FreeTipsCount = table.Column<int>(type: "integer", nullable: true, comment: "Количество бесплатных подсказок"),
                    PaidTipsCount = table.Column<int>(type: "integer", nullable: true, comment: "Количество оплаченных подсказок"),
                    TotalTipsCount = table.Column<int>(type: "integer", nullable: true, comment: "Общее количество подсказок"),
                    TotalPrice = table.Column<decimal>(type: "numeric", nullable: true, comment: "Общая стоимость"),
                    StartDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата начала действия тарифа"),
                    EndDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Дата окончания действия тарифа"),
                    HideDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Дата сокрытия тарифа")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tariff", x => x.Id);
                },
                comment: "Объект - тариф");

            migrationBuilder.CreateIndex(
                name: "IX_Tariff_Name",
                schema: "data",
                table: "Tariff",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tariff",
                schema: "data");
        }
    }
}
