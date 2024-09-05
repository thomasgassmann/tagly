using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Tagly.Api.Services;
using Microsoft.EntityFrameworkCore;
using Tagly.Db;

var builder = WebApplication.CreateBuilder(args);

var dbPath = builder.Configuration["DbPath"];
builder.Services.AddDbContext<TaglyContext>(opt =>
    opt.UseSqlite($"Data Source={dbPath}" ));
builder.Services.AddGrpc(options =>
{
    options.MaxReceiveMessageSize = int.MaxValue;
    options.MaxSendMessageSize = int.MaxValue;
});
builder.Services.AddGrpcReflection();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();

if (!File.Exists(dbPath))
{
    using var serviceScope = app.Services.CreateScope();
    var context = serviceScope.ServiceProvider.GetRequiredService<TaglyContext>();
    context.Database.Migrate();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<AuthService>().AllowAnonymous();
app.MapGrpcService<PhotosService>().RequireAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
app.Run();
