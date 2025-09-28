using Billing.Application.Services;
using Billing.Infrastructure.Repositories;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddScoped<IClientRepository>(sp =>
    new ClientRepository(builder.Configuration.GetConnectionString("DefaultConnection")!)
);
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IProductRepository>(sp =>
    new ProductRepository(builder.Configuration.GetConnectionString("DefaultConnection")!)
);
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Billing API", Version = "v1" });
});

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Billing API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
