using IssueTracker.Application.Services;
using IssueTracker.Infrastructure.Context;
using IssueTracker.Infrastructure.Entities;
using IssueTracker.Infrastructure.Repositories;
using IssueTracker.UI.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContextFactory<IssueTrackerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IDbContextFactory<IssueTrackerDbContext>>().CreateDbContext());

builder.Services.AddScoped<IssueRepository>();
builder.Services.AddScoped<IssueService>();
builder.Services.AddScoped<CsvExportService>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddScoped<AuthService>();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddMudServices();

var app = builder.Build();

// Ensure Database schema & Seed data exist on startup (with retry for SQL Server container boot)
using (var scope = app.Services.CreateScope())
{
    var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<IssueTrackerDbContext>>();
    using var db = contextFactory.CreateDbContext();

    int maxRetries = 10;
    while (maxRetries > 0)
    {
        try
        {
            if (db.Database.EnsureCreated() || !db.Users.Any())
            {
                SeedDatabase(db);
            }
            break;
        }
        catch (Exception ex)
        {
            maxRetries--;
            if (maxRetries == 0)
            {
                Console.WriteLine($"Database initialization failed: {ex.Message}");
                throw;
            }
            Console.WriteLine($"Waiting for SQL Server to become ready... ({maxRetries} retries left)");
            Thread.Sleep(3000);
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

// Helper method to seed initial demo data
static void SeedDatabase(IssueTrackerDbContext db)
{
    if (db.Users.Any()) return;

    var admin = new User { FullName = "Alex Rivers", Email = "alex@issuetracker.io", Role = "Admin", AvatarUrl = "https://i.pravatar.cc/150?u=alex" };
    var dev = new User { FullName = "Jordan Lee", Email = "jordan@issuetracker.io", Role = "Developer", AvatarUrl = "https://i.pravatar.cc/150?u=jordan" };
    var qa = new User { FullName = "Morgan Taylor", Email = "morgan@issuetracker.io", Role = "QA Tester", AvatarUrl = "https://i.pravatar.cc/150?u=morgan" };

    db.Users.AddRange(admin, dev, qa);
    db.SaveChanges();

    var project = new Project { Name = "Core Platform v2", KeyPrefix = "ENG", Description = "Enterprise grade architecture migration." };
    db.Projects.Add(project);
    db.SaveChanges();

    var tagFeat = new Tag { Name = "Feature", ColorHex = "#3b82f6" };
    var tagBug = new Tag { Name = "Bug", ColorHex = "#ef4444" };
    var tagPerf = new Tag { Name = "Performance", ColorHex = "#10b981" };

    db.Tags.AddRange(tagFeat, tagBug, tagPerf);
    db.SaveChanges();

    var issue1 = new Issue
    {
        ProjectId = project.ProjectId,
        IssueKey = "ENG-101",
        Title = "Implement JWT authentication handler",
        Status = "InReview",
        Priority = "High",
        AssigneeId = dev.UserId,
        ReporterId = admin.UserId,
        Tags = new List<Tag> { tagFeat }
    };

    var issue2 = new Issue
    {
        ProjectId = project.ProjectId,
        IssueKey = "ENG-102",
        Title = "Fix database connection timeout under high load",
        Status = "InProgress",
        Priority = "Critical",
        AssigneeId = admin.UserId,
        ReporterId = dev.UserId,
        Tags = new List<Tag> { tagBug, tagPerf }
    };

    var issue3 = new Issue
    {
        ProjectId = project.ProjectId,
        IssueKey = "ENG-103",
        Title = "Optimize SQL queries for Kanban aggregate view",
        Status = "Backlog",
        Priority = "Medium",
        AssigneeId = dev.UserId,
        ReporterId = admin.UserId,
        Tags = new List<Tag> { tagPerf }
    };

    var issue4 = new Issue
    {
        ProjectId = project.ProjectId,
        IssueKey = "ENG-104",
        Title = "Set up CI/CD pipeline on GitHub Actions",
        Status = "InReview",
        Priority = "Low",
        AssigneeId = admin.UserId,
        ReporterId = admin.UserId
    };

    var issue5 = new Issue
    {
        ProjectId = project.ProjectId,
        IssueKey = "ENG-105",
        Title = "Verify responsive layout on mobile screens",
        Status = "Done",
        Priority = "Medium",
        AssigneeId = qa.UserId,
        ReporterId = admin.UserId
    };

    db.Issues.AddRange(issue1, issue2, issue3, issue4, issue5);
    db.SaveChanges();
}