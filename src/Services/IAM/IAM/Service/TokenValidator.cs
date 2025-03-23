namespace Iam.Service;

public class TokenValidator : ITokenValidator
{
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public TokenValidator(TokenValidationParameters tokenValidationParameters)
    {
        _tokenValidationParameters = tokenValidationParameters;
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    public async Task<string?> ValidateTokenAsync(string token)
    {
        try
        {
            var principal = _tokenHandler.ValidateToken(token, _tokenValidationParameters, out _);
            var userId = principal.FindFirst("sub")?.Value; // Lấy userId từ claim "sub"
            return userId;
        }
        catch
        {
            return null; // Token không hợp lệ
        }
    }
}