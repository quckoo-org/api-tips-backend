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

        // Константа валюты для тарифов
        builder.Entity<Tariff>()
            .Property(b => b.Currency)
            .HasDefaultValue("USD");

        // Вычисление стоимости одной подсказки
        builder.Entity<Tariff>()
            .Property(b => b.TipPrice)
            .HasComputedColumnSql(
            "CASE WHEN ISNULL(NULLIF([PaidTipsCount], 0), 0) > 0 THEN [TotalPrice] / [PaidTipsCount] ELSE 0"
            , false);

        // Вычисление общего количества подсказок
        builder.Entity<Tariff>()
            .Property(b => b.TotalTipsCount)
            .HasComputedColumnSql("ISNULL([FreeTipsCount], 0) + ISNULL([PaidTipsCount], 0)", false);

        base.OnModelCreating(builder);
    }
}