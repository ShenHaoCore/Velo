using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloApp.Api.Shared;

namespace VeloApp.Api.Features.Tasks;

// ---- Query ----
public sealed record GetTasksQuery(int Page = 1, int PageSize = 50)
    : IRequest<Result<PagedList<TaskDto>>>;

public sealed record TaskDto(Guid Id, string Title, bool IsCompleted);

// ---- Handler ----
internal sealed class GetTasksHandler(AppDbContext db)
    : IRequestHandler<GetTasksQuery, Result<PagedList<TaskDto>>>
{
    private const int MaxPageSize = 100;

    public async Task<Result<PagedList<TaskDto>>> Handle(
        GetTasksQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1
            ? 50
            : Math.Min(request.PageSize, MaxPageSize);

        var query = db.Tasks.AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(t => t.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TaskDto(t.Id, t.Title, t.IsCompleted))
            .ToListAsync(cancellationToken);

        return new PagedList<TaskDto>(items, page, pageSize, totalCount);
    }
}

// ---- Endpoint ----
public static class GetTasksEndpoint
{
    public static RouteHandlerBuilder MapGetTasks(this IEndpointRouteBuilder app) =>
        app.MapGet("/tasks", (int? page, int? pageSize, ISender sender, CancellationToken ct) =>
                sender.Send(new GetTasksQuery(page ?? 1, pageSize ?? 50), ct)
                    .ToHttpResultAsync())
            .WithName("GetTasks")
            .WithTags("Tasks")
            .Produces<Result<PagedList<TaskDto>>>(StatusCodes.Status200OK);
}
