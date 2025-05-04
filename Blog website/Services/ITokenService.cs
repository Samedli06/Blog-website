using Blog_website.Models;

namespace Blog_website.Services
{
    public interface ITokenService
    {
        string GenerateJwtToken(User user);
        int? ValidateJwtToken(string token);
    }
}
