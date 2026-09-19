using MediatR;
using Microsoft.EntityFrameworkCore;
using Velo.Api.Shared;

namespace Velo.Api.Features.Tasks;

// ---- Query ----
public sealed record GetTasksQuery : IRequest<Result<IReadOnlyList<TaskDto>>>;

public sealed record TaskDto(Guid Id, string Title, bool IsCompleted);

// ---- Handler ----
internal sealed class GetTasksHandler(AppDbContext db)
    : IRequestHandler<GetTasksQuery, Result<IReadOnlyList<TaskDto>>>
{
    // 返回类型须写全名，避免与实体 Task 冲突
    public async System.Threading.Tasks.Task<Result<IReadOnlyList<TaskDto>>> Handle(
        GetTasksQuery request,
        CancellationToken cancellationToken)
    {
        var items = await db.Tasks
            .AsNoTracking()
            .OrderBy(t => t.Title)
            .Select(t => new TaskDto(t.Id, t.Title, t.IsCompleted))
            .ToListAsync(cancellationToken);

        return items;
    }
}

// ---- Endpoint：直接绑定到 MediatR Send ----
public static class GetTasksEndpoint
{
    public static RouteHandlerBuilder MapGetTasks(this IEndpointRouteBuilder app) =>
        app.MapGet("/tasks", (ISender sender, CancellationToken ct) =>
                sender.Send(new GetTasksQuery(), ct))
            .WithName("GetTasks")
            .WithTags("Tasks");
}
