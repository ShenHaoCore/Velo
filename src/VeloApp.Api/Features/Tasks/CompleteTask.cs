using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloApp.Api.Shared;

namespace VeloApp.Api.Features.Tasks;

// ---- Command ----
public sealed record CompleteTaskCommand(Guid Id) : IRequest<Result<TaskDto>>;

// ---- Handler ----
internal sealed class CompleteTaskHandler(AppDbContext db)
    : IRequestHandler<CompleteTaskCommand, Result<TaskDto>>
{
    public async Task<Result<TaskDto>> Handle(
        CompleteTaskCommand request,
        CancellationToken cancellationToken)
    {
        var task = await db.Tasks
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task is null)
            return Result<TaskDto>.Failure("Task not found.");

        if (task.IsCompleted)
            return new TaskDto(task.Id, task.Title, task.IsCompleted);

        task.IsCompleted = true;
        await db.SaveChangesAsync(cancellationToken);

        return new TaskDto(task.Id, task.Title, task.IsCompleted);
    }
}

// ---- Endpoint ----
public static class CompleteTaskEndpoint
{
    public static RouteHandlerBuilder MapCompleteTask(this IEndpointRouteBuilder app) =>
        app.MapPost("/tasks/{id:guid}/complete", (Guid id, ISender sender, CancellationToken ct) =>
                sender.Send(new CompleteTaskCommand(id), ct)
                    .ToHttpResultAsync(failureStatusCode: StatusCodes.Status404NotFound))
            .WithName("CompleteTask")
            .WithTags("Tasks")
            .Produces<Result<TaskDto>>(StatusCodes.Status200OK)
            .Produces<Result<TaskDto>>(StatusCodes.Status404NotFound);
}
