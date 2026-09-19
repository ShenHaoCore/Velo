using MediatR;

namespace VeloApp.Api.Features.Notifications;

/// <summary>
/// 任务创建后的进程内事件；其它模块通过 INotificationHandler 订阅。
/// </summary>
public sealed record TaskCreatedNotification(Guid TaskId, string Title) : INotification;

internal sealed class TaskCreatedNotificationHandler
    : INotificationHandler<TaskCreatedNotification>
{
    public Task Handle(TaskCreatedNotification notification, CancellationToken cancellationToken)
    {
        // Demo：用控制台打印模拟发送通知
        Console.WriteLine(
            $"[Notification] Task created: {notification.TaskId} - {notification.Title}");

        return Task.CompletedTask;
    }
}
