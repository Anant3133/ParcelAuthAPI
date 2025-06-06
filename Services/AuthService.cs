using BCrypt.Net;
using ParcelAuthAPI.Models;
using ParcelAuthAPI.Data;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public string? Register(RegisterModel model)
    {
        if (_context.Users.Any(u => u.Email == model.Email)) return null;

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = model.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            Role = model.Role
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return GenerateJwtToken(user);
    }

    public string? Login(LoginModel model)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            return null;

        return GenerateJwtToken(user);
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
{
    new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", user.Id),
    new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", user.Email),
    new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", user.Role)
};


        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
