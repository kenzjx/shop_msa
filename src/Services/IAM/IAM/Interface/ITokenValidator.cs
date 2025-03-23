namespace IAM.Interface;

public interface ITokenValidator
{
    Task<string?> ValidateTokenAsync(string token);
}