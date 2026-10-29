using Conflux.Modules.Workspaces.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Conflux.Modules.Workspaces.Features.GetWorkspaces;

internal static class GetWorkspaces
{
    internal static void MapGetWorkspaces(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/workspaces", async (WorkspacesDbContext db, CancellationToken cancellationToken) =>
        {
            var workspaces = await db.Workspaces
                .AsNoTracking()
                .OrderByDescending(workspace => workspace.CreatedAtUtc)
                .ThenBy(workspace => workspace.Id)
                .Select(workspace => new WorkspaceListItem(workspace.Id, workspace.Name, workspace.CreatedAtUtc, workspace.Version))
                .ToListAsync(cancellationToken);

            return Results.Ok(workspaces);
        });
    }
}

internal sealed record WorkspaceListItem(Guid Id, string Name, DateTimeOffset CreatedAtUtc, long Version);
