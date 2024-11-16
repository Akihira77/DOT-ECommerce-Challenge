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
        return this.passwordHasher.HashPassword(null, password);
    }

    public bool VerifyPassword(string hashedPassword, string password)
    {
        var result = this.passwordHasher.VerifyHashedPassword(null, hashedPassword, password);
        return result == PasswordVerificationResult.Success;
    }
}


