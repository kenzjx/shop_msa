using Duende.IdentityServer.Models;
using IAM.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Kết nối database (ví dụ SQL Server, chuỗi kết nối trong appsettings.json)
builder.Services.AddDbContext<IamDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Thêm dịch vụ Identity (sử dụng EF Core)
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<IamDbContext>()
    .AddDefaultTokenProviders();

// Cấu hình IdentityServer (chỉ dùng In-Memory cho demo)
builder.Services.AddIdentityServer()
    .AddAspNetIdentity<IdentityUser>()
    .AddInMemoryApiScopes(new[]
    {
        new ApiScope("shop", "My API #1")
    })
    .AddInMemoryClients(new[]
    {
        // Client sử dụng Resource Owner Password
        new Client
        {
            ClientId = "ro.client",  
            AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
            ClientSecrets = { new Secret("secret".Sha256()) },
            AllowedScopes = { "shop" }
        }
    })
    .AddDeveloperSigningCredential();

// cấu hình xác thực
builder.Services.AddAuthentication()
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "http://localhost:5000"; 
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateAudience = false
        };
    });
builder.Services.AddAuthorization();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Tạo cấu trúc DB nếu chưa có (chạy migration)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IamDbContext>();
    db.Database.Migrate(); // Tạo DB + bảng AspNetUsers, AspNetRoles... nếu chưa có
}

// 3. Kích hoạt IdentityServer
app.UseIdentityServer();

// 4. Kích hoạt Authentication/Authorization
app.UseAuthentication();
app.UseAuthorization();

// app.UseHttpsRedirection();
