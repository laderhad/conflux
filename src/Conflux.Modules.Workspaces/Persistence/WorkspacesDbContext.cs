using Conflux.Modules.Workspaces.Domain;
using Microsoft.EntityFrameworkCore;

namespace Conflux.Modules.Workspaces.Persistence;

public sealed class WorkspacesDbContext(DbContextOptions<WorkspacesDbContext> options) : DbContext(options)
{
    public DbSet<Workspace> Workspaces => Set<Workspace>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("workspaces");
        modelBuilder.Entity<Workspace>(entity =>
        {
            entity.ToTable("workspaces");
            entity.HasKey(workspace => workspace.Id);
            entity.Property(workspace => workspace.Name).HasMaxLength(80).IsRequired();
            entity.Property(workspace => workspace.CreatedAtUtc).IsRequired();
            entity.Property(workspace => workspace.Version).HasDefaultValue(1L).IsConcurrencyToken();
            entity.HasIndex(workspace => new { workspace.CreatedAtUtc, workspace.Id });
        });
    }
}
