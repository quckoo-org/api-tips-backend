using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ApiTips.Api.Migrations
{
    /// <inheritdoc />
    public partial class Inittransferfromts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "system");

            migrationBuilder.CreateTable(
                name: "Method",
                schema: "system",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "Уникальный идентификатор метода")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Уникальное название метода")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Method", x => x.Id);
                },
                comment: "Объект - метод");

            migrationBuilder.CreateTable(
                name: "Permission",
                schema: "system",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "Уникальный идентификатор разрешения")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Уникальное название разрешения")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permission", x => x.Id);
                },
                comment: "Объект - разрешение");

            migrationBuilder.CreateTable(
                name: "Role",
                schema: "system",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "Уникальный идентификатор роли")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Уникальное название роли")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                },
                comment: "Объект - пользователь");

            migrationBuilder.CreateTable(
                name: "User",
                schema: "system",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "Уникальный идентификатор пользователя")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Уникальная почта пользователя"),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "Телефон пользователя"),
                    Password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Пароль пользователя в зашифрованном виде (SHA256)"),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Имя пользователя"),
                    SecondName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Отчество пользователя"),
                    LastName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Фамилия пользователя"),
                    Cca3 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Код страны пользователя (ISO 3166-1 alpha-3)"),
                    VerifyDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Дата верификации записи в БД по UTC"),
                    LockDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Дата блокировки записи в БД по UTC"),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Дата удаления записи в БД по UTC"),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()", comment: "Дата создания записи в БД по UTC")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                },
                comment: "Объект - пользователь");

            migrationBuilder.CreateTable(
                name: "MethodPermission",
                schema: "system",
                columns: table => new
                {
                    MethodsId = table.Column<long>(type: "bigint", nullable: false),
                    PermissionsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MethodPermission", x => new { x.MethodsId, x.PermissionsId });
                    table.ForeignKey(
                        name: "FK_MethodPermission_Method_MethodsId",
                        column: x => x.MethodsId,
                        principalSchema: "system",
                        principalTable: "Method",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MethodPermission_Permission_PermissionsId",
                        column: x => x.PermissionsId,
                        principalSchema: "system",
                        principalTable: "Permission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PermissionRole",
                schema: "system",
                columns: table => new
                {
                    PermissionsId = table.Column<long>(type: "bigint", nullable: false),
                    RolesId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionRole", x => new { x.PermissionsId, x.RolesId });
                    table.ForeignKey(
                        name: "FK_PermissionRole_Permission_PermissionsId",
                        column: x => x.PermissionsId,
                        principalSchema: "system",
                        principalTable: "Permission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissionRole_Role_RolesId",
                        column: x => x.RolesId,
                        principalSchema: "system",
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleUser",
                schema: "system",
                columns: table => new
                {
                    RolesId = table.Column<long>(type: "bigint", nullable: false),
                    UsersId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleUser", x => new { x.RolesId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_RoleUser_Role_RolesId",
                        column: x => x.RolesId,
                        principalSchema: "system",
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleUser_User_UsersId",
                        column: x => x.UsersId,
                        principalSchema: "system",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Method_Name",
                schema: "system",
                table: "Method",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MethodPermission_PermissionsId",
                schema: "system",
                table: "MethodPermission",
                column: "PermissionsId");

            migrationBuilder.CreateIndex(
                name: "IX_Permission_Name",
                schema: "system",
                table: "Permission",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PermissionRole_RolesId",
                schema: "system",
                table: "PermissionRole",
                column: "RolesId");

            migrationBuilder.CreateIndex(
                name: "IX_Role_Name",
                schema: "system",
                table: "Role",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleUser_UsersId",
                schema: "system",
                table: "RoleUser",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                schema: "system",
                table: "User",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MethodPermission",
                schema: "system");

            migrationBuilder.DropTable(
                name: "PermissionRole",
                schema: "system");

            migrationBuilder.DropTable(
                name: "RoleUser",
                schema: "system");

            migrationBuilder.DropTable(
                name: "Method",
                schema: "system");

            migrationBuilder.DropTable(
                name: "Permission",
                schema: "system");

            migrationBuilder.DropTable(
                name: "Role",
                schema: "system");

            migrationBuilder.DropTable(
                name: "User",
                schema: "system");
        }
    }
}
