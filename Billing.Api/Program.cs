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

builder.Services.AddScoped<IInvoiceRepository>(sp =>
    new InvoiceRepository(builder.Configuration.GetConnectionString("DefaultConnection")!)
);
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Billing API", Version = "v1" });
});

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") // frontend Angular
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors(MyAllowSpecificOrigins);
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
