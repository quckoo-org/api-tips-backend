using System.Reflection;
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
    ///     Создание модели
    /// </summary>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Дата создания записи в БД по UTC
        builder.Entity<User>()
            .Property(b => b.CreateDateTime)
            .HasDefaultValueSql("now()");

        base.OnModelCreating(builder);
    }
}