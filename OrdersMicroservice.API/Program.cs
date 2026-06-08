using BusinessLogicLayer;
using DataAccessLayer;
using FluentValidation.AspNetCore;
using OrdersMicroservice.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicAccessLayer();
builder.Services.AddControllers();
//FluentValidation
builder.Services.AddFluentValidationAutoValidation();

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
var app = builder.Build();
app.UseExceptionHandlingMiddleware();
app.UseRouting();
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();

//Auth
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

//Endpoint
app.MapControllers();
app.Run();