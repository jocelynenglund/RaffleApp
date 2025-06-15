using FastEndpoints;
using FastEndpoints.Swagger;
using RaffleDraw;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument();
builder.Services.InstallRaffle();
var app = builder.Build();

app.UseHttpsRedirection();
app.UseFastEndpoints().UseSwaggerGen();

app.Run();

public partial class Program { }
