using BarbeariaAPI.Data;
using BarbeariaAPI.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("Data Source=barbearia.db"));

var app = builder.Build();

app.MapGet("/", () => "API Barbearia funcionando");


// CLIENTES
app.MapPost("/clientes", async (Cliente c, AppDbContext db) =>
{
    db.Clientes.Add(c);
    await db.SaveChangesAsync();
    return Results.Ok(c);
});

app.MapGet("/clientes", async (AppDbContext db) =>
    await db.Clientes.ToListAsync());

app.MapPut("/clientes/{id}", async (int id, Cliente input, AppDbContext db) =>
{
    var c = await db.Clientes.FindAsync(id);
    if (c == null) return Results.NotFound();

    c.Nome = input.Nome;
    await db.SaveChangesAsync();
    return Results.Ok(c);
});

app.MapDelete("/clientes/{id}", async (int id, AppDbContext db) =>
{
    var c = await db.Clientes.FindAsync(id);
    if (c == null) return Results.NotFound();

    db.Clientes.Remove(c);
    await db.SaveChangesAsync();
    return Results.Ok();
});


// SERVIÇOS
app.MapPost("/servicos", async (Servico s, AppDbContext db) =>
{
    db.Servicos.Add(s);
    await db.SaveChangesAsync();
    return Results.Ok(s);
});

app.MapGet("/servicos", async (AppDbContext db) =>
    await db.Servicos.ToListAsync());

    app.MapPut("/servicos/{id}", async (int id, Servico input, AppDbContext db) =>
{
    var s = await db.Servicos.FindAsync(id);
    if (s == null) return Results.NotFound();

    s.Nome = input.Nome;
    s.Preco = input.Preco;

    await db.SaveChangesAsync();
    return Results.Ok(s);
});

app.MapDelete("/servicos/{id}", async (int id, AppDbContext db) =>
{
    var s = await db.Servicos.FindAsync(id);
    if (s == null) return Results.NotFound();

    db.Servicos.Remove(s);
    await db.SaveChangesAsync();

    return Results.Ok();
});


// AGENDAMENTOS
app.MapPost("/agendamentos", async (Agendamento a, AppDbContext db) =>
{
    var cliente = await db.Clientes.FindAsync(a.ClienteId);
    var servico = await db.Servicos.FindAsync(a.ServicoId);

    if (cliente == null || servico == null)
        return Results.BadRequest("Cliente ou Serviço inválido");

    var existe = await db.Agendamentos
        .AnyAsync(x => x.Data == a.Data && x.ClienteId == a.ClienteId);

    if (existe)
        return Results.BadRequest("Cliente já tem agendamento nesse horário");

    db.Agendamentos.Add(a);
    await db.SaveChangesAsync();

    return Results.Ok(a);
});

app.MapGet("/agendamentos", async (AppDbContext db) =>
    await db.Agendamentos
        .Include(a => a.Cliente)
        .Include(a => a.Servico)
        .ToListAsync());

app.Urls.Clear();
app.Urls.Add("http://0.0.0.0:5000");

app.Run();