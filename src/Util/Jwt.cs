namespace ECommerce.Util;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

public class JwtService
{
    private readonly IConfiguration configuration;

    public JwtService(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public string GenerateJwtToken(string email)
    {
        var secretKey = this.configuration.GetValue<string>("JWT:Key");
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("JWT secret key not found in environment variables.");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
                new[]{
                    new Claim(ClaimTypes.Email, email)
                }),
            Expires = DateTime.UtcNow.AddDays(1),
            NotBefore = DateTime.Now,
            Issuer = this.configuration.GetValue<string>("JWT:Issuer"),
            Audience = this.configuration.GetValue<string>("JWT:Audience"),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal? VerifyJwtToken(string token)
    {
        var secretKey = this.configuration.GetValue<string>("JWT:Key");
        if (!string.IsNullOrEmpty(secretKey))
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidIssuer = this.configuration.GetValue<string>("JWT:Issuer"),
                ValidAudience = this.configuration.GetValue<string>("JWT:Audience"),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
            };

            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
            return principal;
        }

        return null;
    }
}
