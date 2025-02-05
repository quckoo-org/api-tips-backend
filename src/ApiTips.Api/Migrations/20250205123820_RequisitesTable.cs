using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ApiTips.Api.Migrations
{
    /// <inheritdoc />
    public partial class RequisitesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Requisite",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "Уникальный идентификатор заказа")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsBanned = table.Column<bool>(type: "boolean", nullable: false, comment: "Признак запрета счета"),
                    PaymentType = table.Column<int>(type: "integer", nullable: false, comment: "Статус заказа"),
                    PaymentRequisites = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requisite", x => x.Id);
                },
                comment: "Объект - реквизиты для оплаты");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Requisite",
                schema: "data");
        }
    }
}
