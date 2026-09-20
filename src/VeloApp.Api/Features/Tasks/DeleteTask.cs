using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloApp.Api.Shared;

namespace VeloApp.Api.Features.Tasks;

// ---- Command ----
public sealed record DeleteTaskCommand(Guid Id) : IRequest<Result>;

// ---- Handler ----
internal sealed class DeleteTaskHandler(AppDbContext db)
    : IRequestHandler<DeleteTaskCommand, Result>
{
    public async Task<Result> Handle(
        DeleteTaskCommand request,
        CancellationToken cancellationToken)
    {
        var task = await db.Tasks
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task is null)
            return Result.Failure("Task not found.");

        db.Tasks.Remove(task);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

// ---- Endpoint ----
public static class DeleteTaskEndpoint
{
    public static RouteHandlerBuilder MapDeleteTask(this IEndpointRouteBuilder app) =>
        app.MapDelete("/tasks/{id:guid}", (Guid id, ISender sender, CancellationToken ct) =>
                sender.Send(new DeleteTaskCommand(id), ct)
                    .ToHttpResultAsync(
                        successStatusCode: StatusCodes.Status204NoContent,
                        failureStatusCode: StatusCodes.Status404NotFound))
            .WithName("DeleteTask")
            .WithTags("Tasks")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<Result>(StatusCodes.Status404NotFound);
}
