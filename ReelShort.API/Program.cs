using ReelShort.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container (DI)
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// Configure HTTP request pipeline (Middleware)
app.ConfigurePipeline();

app.Run();