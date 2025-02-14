using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiTips.Api.Migrations
{
    /// <inheritdoc />
    public partial class InvoiceStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "data",
                table: "Invoice",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Статус счета");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                schema: "data",
                table: "Invoice");
        }
    }
}
