using Microsoft.EntityFrameworkCore;
using TaskEntity = Velo.Api.Features.Tasks.Task;

namespace Velo.Api.Shared;

/// <summary>
/// 单一 DbContext：轻量模块化单体里直接注入使用，不引入仓储抽象。
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // DbSet 须与实体同为 internal，避免可访问性不一致
    internal DbSet<TaskEntity> Tasks => Set<TaskEntity>();
}
