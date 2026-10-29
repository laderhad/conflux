using Conflux.Modules.Workspaces.Domain;
using Xunit;

namespace Conflux.IntegrationTests;

public sealed class WorkspaceDomainTests
{
    [Fact]
    public void Valid_name_is_trimmed_and_workspace_has_identity_and_utc_timestamp()
    {
        var created = Workspace.TryCreate("  Engineering  ", out var workspace, out var error);

        Assert.True(created);
        Assert.Null(error);
        Assert.NotNull(workspace);
        Assert.NotEqual(Guid.Empty, workspace.Id);
        Assert.Equal("Engineering", workspace.Name);
        Assert.Equal(TimeSpan.Zero, workspace.CreatedAtUtc.Offset);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(80)]
    public void Names_at_the_length_limits_are_valid(int length)
    {
        var created = Workspace.TryCreate($"  {new string('x', length)}  ", out var workspace, out var error);

        Assert.True(created);
        Assert.Null(error);
        Assert.Equal(length, workspace!.Name.Length);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(" ab ")]
    public void Missing_blank_and_short_names_are_invalid(string? name)
    {
        var created = Workspace.TryCreate(name, out var workspace, out var error);

        Assert.False(created);
        Assert.Null(workspace);
        Assert.False(string.IsNullOrEmpty(error));
    }

    [Fact]
    public void Names_over_80_characters_after_trimming_are_invalid()
    {
        var created = Workspace.TryCreate($"  {new string('x', 81)}  ", out var workspace, out var error);

        Assert.False(created);
        Assert.Null(workspace);
        Assert.False(string.IsNullOrEmpty(error));
    }

    [Fact]
    public void Rename_trims_name_without_changing_identity_or_creation_time()
    {
        Workspace.TryCreate("Engineering", out var workspace, out _);
        var id = workspace!.Id;
        var createdAtUtc = workspace.CreatedAtUtc;

        var renamed = workspace.TryRename("  Platform  ", out var error);

        Assert.True(renamed);
        Assert.Null(error);
        Assert.Equal("Platform", workspace.Name);
        Assert.Equal(id, workspace.Id);
        Assert.Equal(createdAtUtc, workspace.CreatedAtUtc);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    [InlineData(" ab ")]
    public void Invalid_rename_leaves_name_unchanged(string? name)
    {
        Workspace.TryCreate("Engineering", out var workspace, out _);

        var renamed = workspace!.TryRename(name, out var error);

        Assert.False(renamed);
        Assert.False(string.IsNullOrEmpty(error));
        Assert.Equal("Engineering", workspace.Name);
    }

    [Fact]
    public void Rename_rejects_more_than_80_characters_after_trimming()
    {
        Workspace.TryCreate("Engineering", out var workspace, out _);

        var renamed = workspace!.TryRename($"  {new string('x', 81)}  ", out var error);

        Assert.False(renamed);
        Assert.False(string.IsNullOrEmpty(error));
        Assert.Equal("Engineering", workspace.Name);
    }
}
