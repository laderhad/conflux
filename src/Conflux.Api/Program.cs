using Conflux.Modules.Workspaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
var frontendOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? (builder.Environment.IsDevelopment() ? ["http://localhost:5173"] : []);
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.WithOrigins(frontendOrigins).AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddWorkspaces(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("Frontend");
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapWorkspaces();

app.Run();

public partial class Program;
