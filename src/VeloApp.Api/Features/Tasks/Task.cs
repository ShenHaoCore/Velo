namespace VeloApp.Api.Features.Tasks;

/// <summary>
/// 任务实体：internal 限制在本程序集内可见，控制模块边界。
/// 类名 Task 与 BCL 冲突时，外部文件用别名或全名引用。
/// </summary>
internal sealed class Task
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}
