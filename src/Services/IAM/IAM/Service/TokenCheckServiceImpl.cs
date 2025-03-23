using System.IdentityModel.Tokens.Jwt;
using Grpc.Core;
using Microsoft.IdentityModel.Tokens;
using IAM.Proto;
using TokenRequest = IAM.Proto.TokenRequest;

namespace IAM.Service;

// Kế thừa lớp sinh ra từ .proto
public class TokenCheckServiceImpl : TokenCheckService.TokenCheckServiceBase
{
    public override Task<TokenReply> ValidateToken(TokenRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return Task.FromResult(new TokenReply
            {
                IsValid = false,
                ErrorMessage = "Token is empty"
            });
        }

        try
        {
            // 1) Parse/Validate JWT local (ở đây fixed key, có thể đọc config)
            var handler = new JwtSecurityTokenHandler();
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                // Ký bằng chuỗi "secret" .Sha256()  => tuỳ theo bạn phát token thế nào
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes("secret_that_matches_IdentityServer")
                ),
                ValidateLifetime = true,
            };

            handler.ValidateToken(request.Token, validationParams, out var validatedToken);

            // Nếu không ném lỗi => token hợp lệ
            return Task.FromResult(new TokenReply
            {
                IsValid = true,
                ErrorMessage = ""
            });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new TokenReply
            {
                IsValid = false,
                ErrorMessage = ex.Message
            });
        }
    }
}
    