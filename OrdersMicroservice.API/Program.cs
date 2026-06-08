using BusinessLogicLayer;
using DataAccessLayer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataAccessLayer();
builder.Services.AddBusinessLogicAccessLayer();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();