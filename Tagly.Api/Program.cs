using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using Tagly.Api.Services;
using Microsoft.EntityFrameworkCore;
using Tagly.Db;

var builder = WebApplication.CreateBuilder(args);

var dbPath = builder.Configuration["DbPath"];
builder.Services.AddDbContext<TaglyContext>(opt =>
    opt.UseSqlite($"Data Source={dbPath}"));
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
builder.Services.Configure<KestrelServerOptions>(options => { options.Limits.MaxRequestBodySize = int.MaxValue; });
builder.Services.AddHostedService<EmailNotificationWorker>();

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

app.MapGet("/", async (TaglyContext dbContext) =>
{
    var count = await dbContext.Photos.CountAsync();
    return $"Currently storing {count} photos";
});
app.Run();