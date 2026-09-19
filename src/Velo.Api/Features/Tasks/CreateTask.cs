using MediatR;
using Velo.Api.Features.Notifications;
using Velo.Api.Shared;

namespace Velo.Api.Features.Tasks;

// ---- Command ----
public sealed record CreateTaskCommand(string Title) : IRequest<Result<Guid>>;

// ---- Handler：直接注入 DbContext，不建仓储 ----
// MediatR 14 接口方法名为 Handle（返回 Task 即异步）；业务侧不另造 HandleAsync 假合规
internal sealed class CreateTaskHandler(AppDbContext db, IPublisher publisher)
    : IRequestHandler<CreateTaskCommand, Result<Guid>>
{
    // 返回类型须写全名，避免与实体 Task 冲突
    public async System.Threading.Tasks.Task<Result<Guid>> Handle(
        CreateTaskCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Result<Guid>.Failure("Title is required.");

        var task = new Task
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            IsCompleted = false
        };

        db.Tasks.Add(task);
        await db.SaveChangesAsync(cancellationToken);

        // 跨模块通信：进程内 INotification，解耦 Notifications 模块
        await publisher.Publish(
            new TaskCreatedNotification(task.Id, task.Title),
            cancellationToken);

        return task.Id;
    }
}

// ---- Endpoint：Minimal API 直接绑定到 MediatR Send ----
public static class CreateTaskEndpoint
{
    public static RouteHandlerBuilder MapCreateTask(this IEndpointRouteBuilder app) =>
        app.MapPost("/tasks", (CreateTaskCommand command, ISender sender, CancellationToken ct) =>
                sender.Send(command, ct))
            .WithName("CreateTask")
            .WithTags("Tasks");
}
