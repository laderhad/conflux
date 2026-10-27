namespace Conflux.Modules.Workspaces.Domain;

public sealed class Workspace
{
    private Workspace() { }

    private Workspace(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static bool TryCreate(string? name, out Workspace? workspace, out string? error)
    {
        var trimmed = name?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed) || trimmed.Length is < 3 or > 80)
        {
            workspace = null;
            error = "Name must be between 3 and 80 characters after trimming.";
            return false;
        }

        workspace = new Workspace(trimmed);
        error = null;
        return true;
    }
}
