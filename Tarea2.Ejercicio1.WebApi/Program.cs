using Microsoft.EntityFrameworkCore;
using Tarea2.Ejercicio1.WebApi;
Console.OutputEncoding = System.Text.Encoding.UTF8; 

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("https://localhost:7374");
builder.Services.AddDbContext<BoePoliceDbContext>();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorWasm", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors("AllowBlazorWasm");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BoePoliceDbContext>();
    await PdfManager.ProcessPDF(dbContext);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/api/candidates/search", async (
    string query,
    BoePoliceDbContext db,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(query))
        return Results.BadRequest("❌ Debes proporcionar un texto de búsqueda.");

    string normalized = query.Trim().ToUpper();

    var candidates = await db.PoliceCandidates
        .AsNoTracking()
        .Where(c =>
            EF.Functions.Like(c.Dni, $"%{normalized}%") || //Agregado hoy
            EF.Functions.Like(c.NamePlusLastName, $"%{normalized}%") ||
            EF.Functions.Like(c.LastNamePlusName, $"%{normalized}%"))
        .Take(5)
        .ToListAsync();
 

    if (ct.IsCancellationRequested)
        return Results.BadRequest("⏹️ Operación cancelada por el cliente.");

    return candidates is null || candidates.Count == 0
        ? Results.NotFound($"No se encontro ningún registro que coincida con '{query}'.")
        : Results.Ok(candidates);

})
.WithName("SearchPoliceCandidate")
.WithDescription("Busca la primera coincidencia de un candidato por nombre o apellidos concatenados (cancelable).")
.WithOpenApi();

app.Run();

