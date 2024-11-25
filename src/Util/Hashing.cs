namespace ECommerce.Util;
using Microsoft.AspNetCore.Identity;

public class PasswordService
{
    private readonly PasswordHasher<object> passwordHasher;

    public PasswordService()
    {
        passwordHasher = new PasswordHasher<object>();
    }

    public string HashPassword(string password)
    {
        return this.passwordHasher.HashPassword(new object(), password);
    }

    public bool VerifyPassword(string hashedPassword, string password)
    {
        var result = this.passwordHasher.VerifyHashedPassword(new object(), hashedPassword, password);
        return result == PasswordVerificationResult.Success;
    }
}


