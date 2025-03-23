using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Validation;
using IAM.Data;
using IAM.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Server.Kestrel.Core;
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
// Đăng ký gRPC
// Cho phép HTTP/2 cho gRPC
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000, o => o.Protocols = HttpProtocols.Http2);
});
builder.Services.AddGrpc();
// Cấu hình IdentityServer (chỉ dùng In-Memory cho demo)
builder.Services.AddIdentityServer(options =>
    {
        // Tắt endpoint token mặc định
        options.Endpoints.EnableTokenEndpoint = false;
    })
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
builder.Services.AddSingleton<TokenValidationParameters>(sp => new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidIssuer = "https://localhost:5097", // Thay bằng issuer của bạn

    ValidateAudience = true,
    ValidAudience = "shop", // Scope hoặc audience của API

    ValidateLifetime = true, // Kiểm tra thời gian sống của token
    ClockSkew = TimeSpan.Zero, // Không cho phép chênh lệch thời gian

    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-secret-key")) // Khóa ký token
});

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
builder.Services.Configure<IdentityOptions> (options => {
    // Thiết lập về Password
    options.Password.RequireDigit = false; // Không bắt phải có số
    options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
    options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
    options.Password.RequireUppercase = false; // Không bắt buộc chữ in
    options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
    options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

    // Cấu hình Lockout - khóa user
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes (5); // Khóa 5 phút
    options.Lockout.MaxFailedAccessAttempts = 5; // Thất bại 5 lầ thì khóa
    options.Lockout.AllowedForNewUsers = true;
    // Tắt yêu cầu xác thực email nếu muốn cho phép đăng nhập ngay
    options.SignIn.RequireConfirmedAccount = false;
    // Cấu hình về User.
    options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true; // Email là duy nhất

    // // Cấu hình đăng nhập.
    // options.SignIn.RequireConfirmedEmail = true; // Cấu hình xác thực địa chỉ email (email phải tồn tại)
    // options.SignIn.RequireConfirmedPhoneNumber = false; // Xác thực số điện thoại

});
builder.Services.AddAuthorization();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// // Tạo cấu trúc DB nếu chưa có (chạy migration)
// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<IamDbContext>();
//     db.Database.Migrate(); // Tạo DB + bảng AspNetUsers, AspNetRoles... nếu chưa có
//     
//     var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
//     // var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//     // db.Database.Migrate();
//
//     // Kiểm tra user "bob" đã tồn tại chưa
//     var existingUser = await userManager.FindByNameAsync("bob");
//     // if(existingUser != null) await userManager.DeleteAsync(existingUser);
//     if (existingUser == null)
//     {
//         var user = new IdentityUser { UserName = "bob", Email = "bob@test.com" };
//         await userManager.CreateAsync(user, "Pass123$");
//     }
// }

// 3. Kích hoạt IdentityServer
app.UseIdentityServer();

// 4. Kích hoạt Authentication/Authorization
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/secure", () => "Secured endpoint")
    .RequireAuthorization();

app.MapPost("/login-test", async (
    LoginRequest request, 
    SignInManager<IdentityUser> signInManager, 
    UserManager<IdentityUser> userManager) =>
{
    var user = await userManager.FindByNameAsync(request.Username);
    if (user == null)
    {
        return Results.BadRequest("User not found");
    }

    var checkPass = await signInManager.CheckPasswordSignInAsync(
        user, 
        request.Password, 
        lockoutOnFailure: true
    );

    if (!checkPass.Succeeded)
    {
        return Results.BadRequest("Invalid password");
    }

    return Results.Ok("Password valid");
});

app.MapPost("/auth/login", async (
    HttpContext httpContext,
    ITokenService tokenService, // Dịch vụ của IdentityServer
    IUserClaimsPrincipalFactory<IdentityUser> claimsFactory,
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    LoginRequest model // record { string Username; string Password; }
) =>
{
    // 1. Tìm user
    var user = await userManager.FindByNameAsync(model.Username);
    if (user == null)
    {
        return Results.BadRequest("User not found");
    }

    // 2. Kiểm tra mật khẩu
    var check = await signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);
    if (!check.Succeeded)
    {
        return Results.BadRequest("Invalid password");
    }

    // 3. Tạo ClaimsPrincipal
    var principal = await claimsFactory.CreateAsync(user);

    // 4. Xây dựng request cấp token
    var tokenRequest = new TokenCreationRequest
    {
        Subject = principal,
        // Thông tin client, scope, v.v. tuỳ bạn
        ValidatedRequest = new ValidatedRequest
        {
            ClientId = "ro.client",
            Subject = principal,
        }
    };

    // 5. Sinh token qua IdentityServer
    var token = await tokenService.CreateAccessTokenAsync(tokenRequest);
    var jwt = await tokenService.CreateSecurityTokenAsync(token);

    return Results.Ok(new
    {
        access_token = jwt,
        token_type = "Bearer",
        // ... thêm expires_in nếu muốn
    });
});
app.MapGrpcService<TokenCheckServiceImpl>();
// app.UseHttpsRedirection();
app.Run();