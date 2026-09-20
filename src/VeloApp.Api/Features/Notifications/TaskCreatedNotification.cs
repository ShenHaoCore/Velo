using MediatR;

namespace VeloApp.Api.Features.Notifications;

/// <summary>
/// 任务创建后的进程内事件；其它模块通过 INotificationHandler 订阅。
/// </summary>
public sealed record TaskCreatedNotification(Guid TaskId, string Title) : INotification;

internal sealed class TaskCreatedNotificationHandler(ILogger<TaskCreatedNotificationHandler> logger)
    : INotificationHandler<TaskCreatedNotification>
{
    public Task Handle(TaskCreatedNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Task created: {TaskId} - {Title}",
            notification.TaskId,
            notification.Title);

        return Task.CompletedTask;
    }
}
