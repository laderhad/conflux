using Conflux.Modules.Workspaces.Features.CreateWorkspace;
using Conflux.Modules.Workspaces.Features.GetWorkspaces;
using Conflux.Modules.Workspaces.Features.RenameWorkspace;
using Conflux.Modules.Workspaces.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Conflux.Modules.Workspaces;

public static class WorkspacesModule
{
    public static IServiceCollection AddWorkspaces(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Workspaces")
            ?? throw new InvalidOperationException("ConnectionStrings:Workspaces is required.");

        services.AddDbContext<WorkspacesDbContext>(options => options.UseNpgsql(connectionString,
            npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "workspaces")));
        return services;
    }

    public static IEndpointRouteBuilder MapWorkspaces(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapCreateWorkspace();
        endpoints.MapGetWorkspaces();
        endpoints.MapRenameWorkspace();
        return endpoints;
    }
}
