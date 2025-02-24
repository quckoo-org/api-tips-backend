﻿// <auto-generated />
using System;
using ApiTips.Dal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ApiTips.Api.Migrations
{
    [DbContext(typeof(ApplicationContext))]
    [Migration("20250219071204_MovedAccessTokenFromOrderToUser")]
    partial class MovedAccessTokenFromOrderToUser
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ApiTips.Dal.schemas.data.Balance", b =>
                {
                    b.Property<long>("Id")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasComment("Уникальный идентификатор баланса");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("FreeTipsCount")
                        .IsConcurrencyToken()
                        .HasColumnType("bigint")
                        .HasComment("Количество бесплатных подсказок");

                    b.Property<long>("PaidTipsCount")
                        .IsConcurrencyToken()
                        .HasColumnType("bigint")
                        .HasComment("Количество оплаченных подсказок");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("Balance", "data", t =>
                        {
                            t.HasComment("Объект - баланс");
                        });
                });

            modelBuilder.Entity("ApiTips.Dal.schemas.data.BalanceHistory", b =>
                {
                    b.Property<long>("Id")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasComment("Уникальный идентификатор записи истории изменения баланса");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("BalanceId")
                        .HasColumnType("bigint");

                    b.Property<long?>("FreeTipsCountChangedTo")
                        .IsConcurrencyToken()
                        .HasColumnType("bigint")
                        .HasComment("Величина изменения количества бесплатных подсказок");

                    b.Property<DateTime>("OperationDateTime")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()")
                        .HasComment("Дата совершения операции");

                    b.Property<int>("OperationType")
                        .IsConcurrencyToken()
                        .HasColumnType("integer")
                        .HasComment("Тип операции (пополнение/списание)");

                    b.Property<long?>("PaidTipsCountChangedTo")
                        .IsConcurrencyToken()
                        .HasColumnType("bigint")
                        .HasComment("Величина изменения количества оплаченных подсказок");

                    b.Property<string>("ReasonDescription")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasColumnType("text")
                        .HasComment("Описание причины изменения баланса (пример: покупка/списание/промо)");

                    b.Property<long>("TotalTipsBalance")
                        .IsConcurrencyToken()
                        .HasColumnType("bigint")
                        .HasComment("Количество подсказок на балансе после выполнения операции");

                    b.HasKey("Id");

                    b.HasIndex("BalanceId");

                    b.ToTable("BalanceHistory", "data", t =>
                        {
                            t.HasComment("Объект - изменение баланса");
                        });
                });

            modelBuilder.Entity("ApiTips.Dal.schemas.data.Invoice", b =>
                {
                    b.Property<Guid>("Id")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasComment("Уникальный идентификатор счета");

                    b.Property<string>("Alias")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasColumnType("text")
                        .HasComment("Алиас счёта, генерирующийся при создании");

                    b.Property<long>("AmountOfRequests")
                        .IsConcurrencyToken()
                        .HasColumnType("bigint")
                        .HasComment("Общее количество запросов");

                    b.Property<DateTime>("CreatedAt")
                        .IsConcurrencyToken()
                        .HasColumnType("timestamp with time zone")
                        .HasComment("Дата создания счета");

                    b.Property<string>("Description")
                        .IsConcurrencyToken()
                        .HasColumnType("text")
                        .HasComment("Комментарий к счету");

                    b.Property<long>("OrderId")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("PayedAt")
                        .IsConcurrencyToken()
                        .HasColumnType("timestamp with time zone")
                        .HasComment("Дата оплаты счета");

                    b.Property<string>("RefNumber")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasColumnType("text")
                        .HasComment("REF-номер заказа, по которому выставлен счет");

                    b.Property<int>("Status")
                        .IsConcurrencyToken()
                        .HasColumnType("integer")
                        .HasComment("Статус счета");

                    b.HasKey("Id");

                    b.HasIndex("OrderId")
                        .IsUnique();

                    b.ToTable("Invoice", "data", t =>
                        {
                            t.HasComment("Объект - заказ");
                        });
                });

            modelBuilder.Entity("ApiTips.Dal.schemas.data.Order", b =>
                {
                    b.Property<long>("Id")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasComment("Уникальный идентификатор заказа");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("CreateDateTime")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()")
                        .HasComment("Дата создания заказа");

                    b.Property<DateTime?>("PaymentDateTime")
                        .IsConcurrencyToken()
                        .HasColumnType("timestamp with time zone")
                        .HasComment("Дата оплаты заказа");

                    b.Property<int>("Status")
                        .IsConcurrencyToken()
                        .HasColumnType("integer")
                        .HasComment("Статус заказа");

                    b.Property<long>("TariffId")
                        .HasColumnType("bigint");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("TariffId");

                    b.HasIndex("UserId");

                    b.ToTable("Order", "data", t =>
                        {
                            t.HasComment("Объект - заказ");
                        });
                });

            modelBuilder.Entity("ApiTips.Dal.schemas.data.Requisite", b =>
                {
                    b.Property<long>("Id")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasComment("Уникальный идентификатор платежных реквизитов");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<bool>("IsBanned")
                        .IsConcurrencyToken()
                        .HasColumnType("boolean")
                        .HasComment("Признак запрета счета");

                    b.Property<int>("PaymentType")
                        .IsConcurrencyToken()
                        .HasColumnType("integer")
                        .HasComment("Тип платежного реквизита");

                    b.HasKey("Id");

                    b.ToTable("Requisite", "data", t =>
                        {
                            t.HasComment("Объект - реквизиты для оплаты");
                        });
                });

            modelBuilder.Entity("ApiTips.Dal.schemas.data.Tariff", b =>
                {
                    b.Property<long>("Id")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasComment("Уникальный идентификатор тарифа");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime?>("ArchiveDateTime")
                        .IsConcurrencyToken()
                        .HasColumnType("timestamp with time zone")
                        .HasComment("Дата архивации тарифа");

                    b.Property<long?>("CreateById")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("CreateDateTime")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()")
                        .HasComment("Дата создания записи в БД по UTC");

                    b.Property<string>("Currency")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(3)
                        .HasColumnType("character varying(3)")
                        .HasComment("Валюта тарифа, ISO 4217");

                    b.Property<DateTime?>("EndDateTime")
                        .IsConcurrencyToken()
                        .HasColumnType("timestamp with time zone")
                        .HasComment("Дата окончания действия тарифа");

                    b.Property<long?>("FreeTipsCount")
                        .IsConcurrencyToken()
                        .HasColumnType("bigint")
                        .HasComment("Количество бесплатных подсказок");

                    b.Property<DateTime?>("HideDateTime")
                        .IsConcurrencyToken()
                        .HasColumnType("timestamp with time zone")
                        .HasComment("Дата сокрытия тарифа");

                    b.Property<string>("Name")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasComment("Уникальное название тарифа");

                    b.Property<long?>("PaidTipsCount")
                        .IsConcurrencyToken()
                        .HasColumnType("bigint")
                        .HasComment("Количество оплаченных подсказок");

                    b.Property<DateTime>("StartDateTime")
                        .IsConcurrencyToken()
                        .HasColumnType("timestamp with time zone")
                        .HasComment("Дата начала действия тарифа");

                    b.Property<decimal>("TotalPrice")
                        .IsConcurrencyToken()
                        .HasColumnType("numeric")
                        .HasComment("Общая стоимость тарифа, вводится менеджером");

                    b.HasKey("Id");

                    b.HasIndex("CreateById");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Tariff", "data", t =>
                        {
                            t.HasComment("Объект - тариф");
                        });
                });

            modelBuilder.Entity("ApiTips.Dal.schemas.system.Method", b =>
                {
                    b.Property<long>("Id")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasComment("Уникальный идентификатор метода");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Name")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasComment("Уникальное название метода");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Method", "system", t =>
                        {
                            t.HasComment("Объект - метод");
                        });
                });

            modelBuilder.Entity("ApiTips.Dal.schemas.system.Permission", b =>
                {
                    b.Property<long>("Id")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasComment("Уникальный идентификатор разрешения");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Name")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasComment("Уникальное название разрешения");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Permission", "system", t =>
                        {
                            t.HasComment("Объект - разрешение");
                        });
                });

            modelBuilder.Entity("ApiTips.Dal.schemas.system.Role", b =>
                {
                    b.Property<long>("Id")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasComment("Уникальный идентификатор роли");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Name")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasComment("Уникальное название роли");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Role", "system", t =>
                        {
                            t.HasComment("Объект - пользователь");
                        });
                });

            modelBuilder.Entity("ApiTips.Dal.schemas.system.User", b =>
                {
                    b.Property<long>("Id")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasComment("Уникальный идентификатор пользователя");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<Guid?>("AccessToken")
                        .IsConcurrencyToken()
                        .HasColumnType("uuid")
                        .HasComment("Токен для получения подсказок");

                    b.Property<string>("Cca3")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasComment("Код страны пользователя (ISO 3166-1 alpha-3)");

                    b.Property<DateTime>("CreateDateTime")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()")
                        .HasComment("Дата создания записи в БД по UTC");

                    b.Property<DateTime?>("DeleteDateTime")
                        .IsConcurrencyToken()
                        .HasColumnType("timestamp with time zone")
                        .HasComment("Дата удаления записи в БД по UTC");

                    b.Property<string>("Email")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasComment("Уникальная почта пользователя");

                    b.Property<string>("FirstName")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasComment("Имя пользователя");

                    b.Property<string>("LastName")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasComment("Фамилия пользователя");

                    b.Property<DateTime?>("LockDateTime")
                        .IsConcurrencyToken()
                        .HasColumnType("timestamp with time zone")
                        .HasComment("Дата блокировки записи в БД по UTC");

                    b.Property<string>("Password")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasComment("Пароль пользователя в зашифрованном виде (SHA256)");

                    b.Property<string>("PhoneNumber")
                        .IsConcurrencyToken()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasComment("Телефон пользователя");

                    b.Property<string>("SecondName")
                        .IsConcurrencyToken()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasComment("Отчество пользователя");

                    b.Property<DateTime?>("VerifyDateTime")
                        .IsConcurrencyToken()
                        .HasColumnType("timestamp with time zone")
                        .HasComment("Дата верификации записи в БД по UTC");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("User", "system", t =>
                        {
                            t.HasComment("Объект - пользователь");
                        });
                });

            modelBuilder.Entity("MethodPermission", b =>
                {
                    b.Property<long>("MethodsId")
                        .HasColumnType("bigint");

                    b.Property<long>("PermissionsId")
                        .HasColumnType("bigint");

                    b.HasKey("MethodsId", "PermissionsId");

                    b.HasIndex("PermissionsId");

                    b.ToTable("MethodPermission", "system");
                });

            modelBuilder.Entity("PermissionRole", b =>
                {
                    b.Property<long>("PermissionsId")
                        .HasColumnType("bigint");

                    b.Property<long>("RolesId")
                        .HasColumnType("bigint");

                    b.HasKey("PermissionsId", "RolesId");

                    b.HasIndex("RolesId");

                    b.ToTable("PermissionRole", "system");
                });

            modelBuilder.Entity("RoleUser", b =>
                {
                    b.Property<long>("RolesId")
                        .HasColumnType("bigint");

                    b.Property<long>("UsersId")
                        .HasColumnType("bigint");

                    b.HasKey("RolesId", "UsersId");

                    b.HasIndex("UsersId");

                    b.ToTable("RoleUser", "system");
                });

            modelBuilder.Entity("ApiTips.Dal.schemas.data.Balance", b =>
                {
                    b.HasOne("ApiTips.Dal.schemas.system.User", "User")
                        .WithOne("Balance")
                        .HasForeignKey("ApiTips.Dal.schemas.data.Balance", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("ApiTips.Dal.schemas.data.BalanceHistory", b =>
                {
                    b.HasOne("ApiTips.Dal.schemas.data.Balance", "Balance")
                        .WithMany("History")
                        .HasForeignKey("BalanceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Balance");
                });

            modelBuilder.Entity("ApiTips.Dal.schemas.data.Invoice", b =>
                {
                    b.HasOne("ApiTips.Dal.schemas.data.Order", "Order")
                        .WithOne("Invoice")
                        .HasForeignKey("ApiTips.Dal.schemas.data.Invoice", "OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("ApiTips.Dal.schemas.data.Invoice+Currency", "CurrentCurrency", b1 =>
                        {
                            b1.Property<Guid>("InvoiceId")
                                .HasColumnType("uuid");

                            b1.Property<string>("CurrencyType")
                                .IsConcurrencyToken()
                                .IsRequired()
                                .HasColumnType("text")
                                .HasComment("Валюта для оплаты");

                            b1.Property<decimal>("TotalAmount")
                                .IsConcurrencyToken()
                                .HasColumnType("numeric")
                                .HasComment("Сумма для оплаты");

                            b1.Property<int>("Type")
                                .IsConcurrencyToken()
                                .HasColumnType("integer")
                                .HasComment("Способ оплаты");

                            b1.HasKey("InvoiceId");

                            b1.ToTable("Invoice", "data");

                            b1.ToJson("CurrentCurrency");

                            b1.WithOwner()
                                .HasForeignKey("InvoiceId");
                        });

                    b.Navigation("CurrentCurrency")
                        .IsRequired();

                    b.Navigation("Order");
                });

            modelBuilder.Entity("ApiTips.Dal.schemas.data.Order", b =>
                {
                    b.HasOne("ApiTips.Dal.schemas.data.Tariff", "Tariff")
                        .WithMany("Orders")
                        .HasForeignKey("TariffId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ApiTips.Dal.schemas.system.User", "User")
                        .WithMany("Orders")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Tariff");

                    b.Navigation("User");
                });

            modelBuilder.Entity("ApiTips.Dal.schemas.data.Requisite", b =>
                {
                    b.OwnsOne("ApiTips.Dal.schemas.data.Requisite+PaymentDetails", "PaymentRequisites", b1 =>
                        {
                            b1.Property<long>("RequisiteId")
                                .HasColumnType("bigint");

                            b1.HasKey("RequisiteId");

                            b1.ToTable("Requisite", "data");

                            b1.ToJson("PaymentRequisites");

                            b1.WithOwner()
                                .HasForeignKey("RequisiteId");

                            b1.OwnsOne("ApiTips.Dal.schemas.data.Requisite+PaymentDetails+BankAccount", "BankAccountDetails", b2 =>
                                {
                                    b2.Property<long>("PaymentDetailsRequisiteId")
                                        .HasColumnType("bigint");

                                    b2.Property<string>("AccountNumber")
                                        .IsConcurrencyToken()
                                        .IsRequired()
                                        .HasColumnType("text")
                                        .HasComment("Номер аккаунта");

                                    b2.Property<string>("AdditionalInfo")
                                        .IsConcurrencyToken()
                                        .HasColumnType("text")
                                        .HasComment("Дополнительная информация");

                                    b2.Property<string>("BankAddress")
                                        .IsConcurrencyToken()
                                        .IsRequired()
                                        .HasColumnType("text")
                                        .HasComment("Адрес банка");

                                    b2.Property<string>("BankName")
                                        .IsConcurrencyToken()
                                        .IsRequired()
                                        .HasColumnType("text")
                                        .HasComment("Наименование банка");

                                    b2.Property<string>("Iban")
                                        .IsConcurrencyToken()
                                        .IsRequired()
                                        .HasColumnType("text")
                                        .HasComment("Iban номер");

                                    b2.Property<string>("Swift")
                                        .IsConcurrencyToken()
                                        .IsRequired()
                                        .HasColumnType("text")
                                        .HasComment("Swift номер");

                                    b2.Property<string>("Type")
                                        .IsConcurrencyToken()
                                        .IsRequired()
                                        .HasColumnType("text")
                                        .HasComment("Валюта счёта");

                                    b2.HasKey("PaymentDetailsRequisiteId");

                                    b2.ToTable("Requisite", "data");

                                    b2.ToJson("BankAccountDetails");

                                    b2.WithOwner()
                                        .HasForeignKey("PaymentDetailsRequisiteId");
                                });

                            b1.OwnsOne("ApiTips.Dal.schemas.data.Requisite+PaymentDetails+CryptoWallet", "CryptoWalletDetails", b2 =>
                                {
                                    b2.Property<long>("PaymentDetailsRequisiteId")
                                        .HasColumnType("bigint");

                                    b2.Property<string>("Address")
                                        .IsConcurrencyToken()
                                        .IsRequired()
                                        .HasColumnType("text")
                                        .HasComment("Номер расчётного счета");

                                    b2.Property<string>("Token")
                                        .IsConcurrencyToken()
                                        .IsRequired()
                                        .HasColumnType("text")
                                        .HasComment("Токен для крипто-кошелька");

                                    b2.Property<string>("Type")
                                        .IsConcurrencyToken()
                                        .IsRequired()
                                        .HasColumnType("text")
                                        .HasComment("Тип крипто-валюты");

                                    b2.Property<string>("Wallet")
                                        .IsConcurrencyToken()
                                        .IsRequired()
                                        .HasColumnType("text")
                                        .HasComment("Справочник крипто-валюты");

                                    b2.HasKey("PaymentDetailsRequisiteId");

                                    b2.ToTable("Requisite", "data");

                                    b2.ToJson("CryptoWalletDetails");

                                    b2.WithOwner()
                                        .HasForeignKey("PaymentDetailsRequisiteId");
                                });

                            b1.Navigation("BankAccountDetails");

                            b1.Navigation("CryptoWalletDetails");
                        });

                    b.Navigation("PaymentRequisites")
                        .IsRequired();
                });

            modelBuilder.Entity("ApiTips.Dal.schemas.data.Tariff", b =>
                {
                    b.HasOne("ApiTips.Dal.schemas.system.User", "CreateBy")
                        .WithMany()
                        .HasForeignKey("CreateById");

                    b.Navigation("CreateBy");
                });

            modelBuilder.Entity("MethodPermission", b =>
                {
                    b.HasOne("ApiTips.Dal.schemas.system.Method", null)
                        .WithMany()
                        .HasForeignKey("MethodsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ApiTips.Dal.schemas.system.Permission", null)
                        .WithMany()
                        .HasForeignKey("PermissionsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PermissionRole", b =>
                {
                    b.HasOne("ApiTips.Dal.schemas.system.Permission", null)
                        .WithMany()
                        .HasForeignKey("PermissionsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ApiTips.Dal.schemas.system.Role", null)
                        .WithMany()
                        .HasForeignKey("RolesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("RoleUser", b =>
                {
                    b.HasOne("ApiTips.Dal.schemas.system.Role", null)
                        .WithMany()
                        .HasForeignKey("RolesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ApiTips.Dal.schemas.system.User", null)
                        .WithMany()
                        .HasForeignKey("UsersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ApiTips.Dal.schemas.data.Balance", b =>
                {
                    b.Navigation("History");
                });

            modelBuilder.Entity("ApiTips.Dal.schemas.data.Order", b =>
                {
                    b.Navigation("Invoice");
                });

            modelBuilder.Entity("ApiTips.Dal.schemas.data.Tariff", b =>
                {
                    b.Navigation("Orders");
                });

            modelBuilder.Entity("ApiTips.Dal.schemas.system.User", b =>
                {
                    b.Navigation("Balance");

                    b.Navigation("Orders");
                });
#pragma warning restore 612, 618
        }
    }
}
