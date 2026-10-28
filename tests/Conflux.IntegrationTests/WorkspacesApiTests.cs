using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Conflux.Modules.Workspaces.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Conflux.IntegrationTests;

public sealed class WorkspacesApiTests(PostgresFixture fixture) : IClassFixture<PostgresFixture>, IAsyncLifetime
{
    public Task InitializeAsync() => fixture.ClearAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Empty_database_returns_empty_list()
    {
        var response = await fixture.Client.GetAsync("/api/workspaces");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty((await response.Content.ReadFromJsonAsync<WorkspaceDto[]>())!);
    }

    [Fact]
    public async Task Create_trims_name_and_list_reads_saved_workspace()
    {
        var create = await fixture.Client.PostAsJsonAsync("/api/workspaces", new { name = "  Engineering  " });

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = (await create.Content.ReadFromJsonAsync<WorkspaceDto>())!;
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("Engineering", created.Name);
        Assert.Equal(TimeSpan.Zero, created.CreatedAtUtc.Offset);

        var list = (await fixture.Client.GetFromJsonAsync<WorkspaceDto[]>("/api/workspaces"))!;
        Assert.Single(list);
        Assert.Equal(created, list[0]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(" ab ")]
    public async Task Invalid_name_returns_validation_problem(string? name)
    {
        var response = await fixture.Client.PostAsJsonAsync("/api/workspaces", new { name });

        await AssertValidationProblem(response);
        Assert.Empty((await fixture.Client.GetFromJsonAsync<WorkspaceDto[]>("/api/workspaces"))!);
    }

    [Fact]
    public async Task Missing_name_returns_validation_problem()
    {
        var response = await fixture.Client.PostAsJsonAsync("/api/workspaces", new { });

        await AssertValidationProblem(response);
    }

    [Fact]
    public async Task More_than_80_characters_after_trimming_is_invalid()
    {
        var response = await fixture.Client.PostAsJsonAsync("/api/workspaces", new { name = $"  {new string('x', 81)}  " });

        await AssertValidationProblem(response);
    }

    [Fact]
    public async Task List_orders_newest_first_and_uses_id_as_tie_breaker()
    {
        var first = (await (await fixture.Client.PostAsJsonAsync("/api/workspaces", new { name = "First" }))
            .Content.ReadFromJsonAsync<WorkspaceDto>())!;
        var second = (await (await fixture.Client.PostAsJsonAsync("/api/workspaces", new { name = "Second" }))
            .Content.ReadFromJsonAsync<WorkspaceDto>())!;
        var older = (await (await fixture.Client.PostAsJsonAsync("/api/workspaces", new { name = "Older" }))
            .Content.ReadFromJsonAsync<WorkspaceDto>())!;

        var recentDate = new DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero);
        await fixture.SetCreatedAtAsync(first.Id, recentDate);
        await fixture.SetCreatedAtAsync(second.Id, recentDate);
        await fixture.SetCreatedAtAsync(older.Id, recentDate.AddDays(-1));

        var list = (await fixture.Client.GetFromJsonAsync<WorkspaceDto[]>("/api/workspaces"))!;
        var expectedIds = new[] { first.Id, second.Id }.OrderBy(id => id).Append(older.Id);
        Assert.Equal(expectedIds, list.Select(item => item.Id));
    }

    [Fact]
    public async Task Health_reports_host_availability()
    {
        var response = await fixture.Client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("http://frontend.test", true)]
    [InlineData("http://other.test", false)]
    public async Task Cors_allows_only_the_configured_frontend_origin(string origin, bool allowed)
    {
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/workspaces");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(allowed, response.Headers.TryGetValues("Access-Control-Allow-Origin", out var origins));
        if (allowed) Assert.Equal(origin, Assert.Single(origins!));
    }

    [Fact]
    public async Task Migration_history_is_owned_by_the_workspaces_schema()
    {
        await using var connection = new NpgsqlConnection(fixture.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "SELECT to_regclass('workspaces.\"__EFMigrationsHistory\"') IS NOT NULL " +
            "AND to_regclass('public.\"__EFMigrationsHistory\"') IS NULL", connection);

        Assert.Equal(true, await command.ExecuteScalarAsync());
    }

    private static async Task AssertValidationProblem(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(json.RootElement.GetProperty("errors").TryGetProperty("name", out var errors));
        Assert.NotEmpty(errors.EnumerateArray());
    }

    private sealed record WorkspaceDto(Guid Id, string Name, DateTimeOffset CreatedAtUtc);
}

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:17-alpine")
        .Build();
    private WebApplicationFactory<Program> _factory = null!;

    public HttpClient Client { get; private set; } = null!;
    public string ConnectionString => _database.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _database.StartAsync();
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Workspaces", ConnectionString);
            builder.UseSetting("Cors:AllowedOrigins:0", "http://frontend.test");
        });
        Client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WorkspacesDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task ClearAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WorkspacesDbContext>();
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE workspaces.workspaces");
    }

    public async Task SetCreatedAtAsync(Guid id, DateTimeOffset createdAtUtc)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WorkspacesDbContext>();
        await db.Workspaces.Where(workspace => workspace.Id == id)
            .ExecuteUpdateAsync(update => update.SetProperty(workspace => workspace.CreatedAtUtc, createdAtUtc));
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await _factory.DisposeAsync();
        await _database.DisposeAsync();
    }
}
