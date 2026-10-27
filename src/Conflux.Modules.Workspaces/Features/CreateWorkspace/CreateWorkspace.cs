using Conflux.Modules.Workspaces.Domain;
using Conflux.Modules.Workspaces.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conflux.Modules.Workspaces.Features.CreateWorkspace;

internal static class CreateWorkspace
{
    internal static void MapCreateWorkspace(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/workspaces", async (CreateWorkspaceRequest request, WorkspacesDbContext db, CancellationToken cancellationToken) =>
        {
            if (!Workspace.TryCreate(request.Name, out var workspace, out var error))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["name"] = [error!]
                });
            }

            db.Workspaces.Add(workspace!);
            await db.SaveChangesAsync(cancellationToken);

            var response = new WorkspaceResponse(workspace!.Id, workspace.Name, workspace.CreatedAtUtc);
            return Results.Json(response, statusCode: StatusCodes.Status201Created);
        });
    }
}

internal sealed record CreateWorkspaceRequest(string? Name);
internal sealed record WorkspaceResponse(Guid Id, string Name, DateTimeOffset CreatedAtUtc);
