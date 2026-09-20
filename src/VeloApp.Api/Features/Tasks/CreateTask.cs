using MediatR;
using VeloApp.Api.Features.Notifications;
using VeloApp.Api.Shared;

namespace VeloApp.Api.Features.Tasks;

// ---- Command ----
public sealed record CreateTaskCommand(string Title) : IRequest<Result<Guid>>;

// ---- Handler：直接注入 DbContext，不建仓储 ----
internal sealed class CreateTaskHandler(AppDbContext db, IPublisher publisher)
    : IRequestHandler<CreateTaskCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateTaskCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Result<Guid>.Failure("Title is required.");

        var title = request.Title.Trim();
        if (title.Length > TodoItem.TitleMaxLength)
            return Result<Guid>.Failure($"Title must be at most {TodoItem.TitleMaxLength} characters.");

        var task = new TodoItem
        {
            Id = Guid.NewGuid(),
            Title = title,
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
                sender.Send(command, ct).ToHttpResultAsync(StatusCodes.Status201Created))
            .WithName("CreateTask")
            .WithTags("Tasks")
            .Produces<Result<Guid>>(StatusCodes.Status201Created)
            .Produces<Result<Guid>>(StatusCodes.Status400BadRequest);
}
