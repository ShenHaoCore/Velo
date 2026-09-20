using Microsoft.EntityFrameworkCore;
using VeloApp.Api.Features.Tasks;

namespace VeloApp.Api.Shared;

/// <summary>
/// 单一 DbContext：轻量模块化单体里直接注入使用，不引入仓储抽象。
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // DbSet 须与实体同为 internal，避免可访问性不一致
    internal DbSet<TodoItem> Tasks => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var todo = modelBuilder.Entity<TodoItem>();
        todo.ToTable("Tasks");
        todo.HasKey(t => t.Id);
        todo.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(TodoItem.TitleMaxLength);
    }
}
