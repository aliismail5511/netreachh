using NetReach.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers & Swagger
builder.Services.AddControllers()
    .AddNewtonsoftJson(); // Important for handling JSON properly
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Services
builder.Services.AddHttpClient<CryptomusService>();
builder.Services.AddSingleton<ProductService>();
builder.Services.AddSingleton<EmailService>();

// CORS - Allow your frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "https://net-reach.vercel.app",
                "https://netreach.site",
                "http://localhost:5173",
                "http://localhost:3000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Swagger - Enable in all environments for testing
app.UseSwagger();
app.UseSwaggerUI();

// IMPORTANT: Order matters!
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

// Log startup info
Console.WriteLine("==============================================");
Console.WriteLine($"🚀 NetReach API Started at {DateTime.UtcNow}");
Console.WriteLine($"📧 Email: info@netreach.site");
Console.WriteLine($"💳 Cryptomus Merchant: edd0baa0-0a28-4eb4-97ca-08f7bbd450d6");
Console.WriteLine("==============================================");

app.Run();