using System.ComponentModel.DataAnnotations;

namespace VeloApp.Api.Features.Tasks;

/// <summary>
/// 待办实体：internal 限制在本程序集内可见，控制模块边界。
/// </summary>
internal sealed class TodoItem
{
    public const int TitleMaxLength = 200;

    public Guid Id { get; set; }

    [MaxLength(TitleMaxLength)]
    public string Title { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }
}
