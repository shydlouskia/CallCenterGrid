using CallCenter.Api.Api;
using CallCenter.Api.Data;
using CallCenter.Api.Import;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlite("Data Source=contacts.db");
});

builder.Services.AddHostedService<ContactsImportHostedService>();

var app = builder.Build();

app.MapGet("/api/health", () => Results.Ok(new { ok = true }));

app.MapGet("/api/contacts", async (
    AppDbContext db,
    string? q,
    string? sortBy,
    string? sortDir,
    int page = 1,
    int pageSize = 50) =>
{
    page = Math.Max(1, page);
    pageSize = Math.Clamp(pageSize, 10, 200);

    var query = db.Contacts.AsNoTracking().AsQueryable();

    if (!string.IsNullOrWhiteSpace(q))
    {
        var term = q.Trim();
        query = query.Where(c =>
            c.FirstName.Contains(term) ||
            c.LastName.Contains(term) ||
            c.Email.Contains(term) ||
            c.Phone.Contains(term) ||
            c.City.Contains(term) ||
            c.State.Contains(term) ||
            c.Status.Contains(term));
    }

    var totalCount = await query.CountAsync();

    sortBy ??= "id";
    if (!SortMap.Map.TryGetValue(sortBy, out var expr))
        expr = SortMap.Map["id"];

    var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
    query = desc
        ? query.OrderByDescending(expr).ThenByDescending(x => x.Id)
        : query.OrderBy(expr).ThenBy(x => x.Id);

    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return Results.Ok(new { items, totalCount, page, pageSize });
});

app.Run("http://localhost:5174");