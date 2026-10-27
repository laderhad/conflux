using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Conflux.Modules.Workspaces.Persistence;

public sealed class WorkspacesDbContextFactory : IDesignTimeDbContextFactory<WorkspacesDbContext>
{
    public WorkspacesDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Workspaces")
            ?? throw new InvalidOperationException("ConnectionStrings__Workspaces is required for migrations.");

        var options = new DbContextOptionsBuilder<WorkspacesDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new WorkspacesDbContext(options);
    }
}
