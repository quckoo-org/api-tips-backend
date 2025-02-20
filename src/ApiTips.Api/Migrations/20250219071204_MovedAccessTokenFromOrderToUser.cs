using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiTips.Api.Migrations
{
    /// <inheritdoc />
    public partial class MovedAccessTokenFromOrderToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessToken",
                schema: "data",
                table: "Order");

            migrationBuilder.AddColumn<Guid>(
                name: "AccessToken",
                schema: "system",
                table: "User",
                type: "uuid",
                nullable: true,
                comment: "Токен для получения подсказок");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessToken",
                schema: "system",
                table: "User");

            migrationBuilder.AddColumn<Guid>(
                name: "AccessToken",
                schema: "data",
                table: "Order",
                type: "uuid",
                nullable: true,
                comment: "Токен для получения подсказок");
        }
    }
}
