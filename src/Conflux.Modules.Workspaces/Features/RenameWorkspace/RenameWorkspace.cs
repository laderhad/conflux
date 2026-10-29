using Conflux.Modules.Workspaces.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Conflux.Modules.Workspaces.Features.RenameWorkspace;

internal static class RenameWorkspace
{
    internal static void MapRenameWorkspace(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPatch("/api/workspaces/{id:guid}", async (
            Guid id, RenameWorkspaceRequest request, WorkspacesDbContext db, CancellationToken cancellationToken) =>
        {
            var workspace = await db.Workspaces.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
            if (workspace is null) return Results.NotFound();

            if (request.ExpectedVersion is null or <= 0)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["expectedVersion"] = ["Expected version must be a positive number."]
                });
            }

            if (!workspace.TryRename(request.Name, out var error))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["name"] = [error!]
                });
            }

            db.Entry(workspace).Property(item => item.Version).OriginalValue = request.ExpectedVersion.Value;
            try
            {
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Results.Conflict();
            }

            return Results.Ok(new RenameWorkspaceResponse(workspace.Id, workspace.Name, workspace.CreatedAtUtc, workspace.Version));
        });
    }
}

internal sealed record RenameWorkspaceRequest(string? Name, long? ExpectedVersion);
internal sealed record RenameWorkspaceResponse(Guid Id, string Name, DateTimeOffset CreatedAtUtc, long Version);
