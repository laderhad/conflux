namespace Conflux.Modules.Workspaces.Domain;

public sealed class Workspace
{
    private const string InvalidNameMessage = "Name must be between 3 and 80 characters after trimming.";

    private Workspace() { }

    private Workspace(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        Version = 1;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public long Version { get; private set; }

    public static bool TryCreate(string? name, out Workspace? workspace, out string? error)
    {
        if (!TryNormalizeName(name, out var normalizedName))
        {
            workspace = null;
            error = InvalidNameMessage;
            return false;
        }

        workspace = new Workspace(normalizedName!);
        error = null;
        return true;
    }

    public bool TryRename(string? name, out string? error)
    {
        if (!TryNormalizeName(name, out var normalizedName))
        {
            error = InvalidNameMessage;
            return false;
        }

        Name = normalizedName!;
        Version++;
        error = null;
        return true;
    }

    private static bool TryNormalizeName(string? name, out string? normalizedName)
    {
        normalizedName = name?.Trim();
        return !string.IsNullOrWhiteSpace(normalizedName) && normalizedName.Length is >= 3 and <= 80;
    }
}
