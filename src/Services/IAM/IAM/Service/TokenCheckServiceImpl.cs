using System.IdentityModel.Tokens.Jwt;
using Grpc.Core;
using Microsoft.IdentityModel.Tokens;
using IAM.Proto;
using TokenRequest = IAM.Proto.TokenRequest;

namespace IAM.Service;

// Kế thừa lớp sinh ra từ .proto
public class TokenCheckService : TokenCheckService.TokenCheckServiceBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IdentityDbContext _dbContext;
    private readonly ITokenValidator _tokenValidator; // Custom service để validate token

    public TokenCheckService(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IdentityDbContext dbContext,
        ITokenValidator tokenValidator)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
        _tokenValidator = tokenValidator;
    }

    public override async Task<TokenReply> ValidateToken(TokenRequest request, ServerCallContext context)
    {
        // Validate token
        var userId = await _tokenValidator.ValidateTokenAsync(request.Token);
        if (userId == null)
        {
            return new TokenReply
            {
                IsValid = false,
                ErrorMessage = "Invalid or expired token."
            };
        }

        // Lấy thông tin người dùng từ IdentityDbContext
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new TokenReply
            {
                IsValid = false,
                ErrorMessage = "User not found."
            };
        }

        // Lấy danh sách roles của người dùng
        var roles = await _userManager.GetRolesAsync(user);

        // Lấy danh sách policies (nếu bạn có custom policies)
        var policies = await _dbContext.UserClaims
            .Where(c => c.UserId == user.Id && c.ClaimType == "policy")
            .Select(c => c.ClaimValue)
            .ToListAsync();

        // Trả về kết quả
        return new TokenReply
        {
            IsValid = true,
            Roles = { roles }, // Gán danh sách roles
            Policies = { policies }, // Gán danh sách policies
            ErrorMessage = string.Empty
        };
    }
}
    