using System.Reflection;
using ApiTips.Dal.schemas.data;
using ApiTips.Dal.schemas.system;
using Microsoft.EntityFrameworkCore;

namespace ApiTips.Dal;

/// <summary>
///     Контекст базы данных
/// </summary>
public class ApplicationContext(DbContextOptions<ApplicationContext> options) : DbContext(options)
{
    /// <summary>
    ///     Пользователи
    ///     Schema = "system"
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    ///     Роли
    ///     Schema = "system"
    /// </summary>
    public DbSet<Role> Roles => Set<Role>();

    /// <summary>
    ///     Разрешения
    ///     Schema = "system"
    /// </summary>
    public DbSet<Permission> Permissions => Set<Permission>();

    /// <summary>
    ///     Методы
    ///     Schema = "system"
    /// </summary>
    public DbSet<Method> Methods => Set<Method>();

    /// <summary>
    ///     Тарифы
    ///     Schema = "data"
    /// </summary>
    public DbSet<Tariff> Tariffs => Set<Tariff>();

    public DbSet<Payment> Payments => Set<Payment>();

    /// <summary>
    ///     Заказы
    ///     Schema = "data"
    /// </summary>
    public DbSet<Order> Orders => Set<Order>();

    /// <summary>
    ///     Создание модели
    /// </summary>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Дата создания записи в БД по UTC
        builder.Entity<User>()
            .Property(b => b.CreateDateTime)
            .HasDefaultValueSql("now()");
        
        // Дата создания записи в БД по UTC
        builder.Entity<Tariff>()
            .Property(b => b.CreateDateTime)
            .HasDefaultValueSql("now()");

        // Дата создания записи в БД по UTC
        builder.Entity<Order>()
            .Property(b => b.CreateDateTime)
            .HasDefaultValueSql("now()");


        // Настройка для jsonB структуры платёжной информации
        builder
            .Entity<Payment>()
            .OwnsOne(param => param.Details, ownedNavigationBuilder =>
            {
                ownedNavigationBuilder.ToJson();
                ownedNavigationBuilder.OwnsOne(param => param.BankAccountDetails, bank =>
                {
                    bank.ToJson();
                });
                ownedNavigationBuilder.OwnsOne(param => param.CryptoWalletDetails, crypto =>
                {
                    crypto.ToJson();
                });
            })
            ;
        
        
        base.OnModelCreating(builder);
    }
}