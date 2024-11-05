using JWT.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(Users user)
    {
        var passwordHasher = new PasswordHasher<Users>();
        user.Password = passwordHasher.HashPassword(user, user.Password);
        if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            return BadRequest("User already exists.");

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok("User registered successfully.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(Users user)
    {
        var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
        if (dbUser == null)
            return Unauthorized("Invalid credentials");

        var passwordHasher = new PasswordHasher<Users>();
        var passwordVerificationResult = passwordHasher.VerifyHashedPassword(dbUser, dbUser.Password, user.Password);
        if (passwordVerificationResult != PasswordVerificationResult.Success)
            return Unauthorized("Invalid credentials");

        var jwtSettings = _configuration.GetSection("Jwt");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, dbUser.Username)
            }),
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["DurationInMinutes"])),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);


        var expires = tokenDescriptor.Expires?.ToString("o"); // ISO 8601 format

        return Ok(new
        {
            token = tokenString,
            expires = expires
        });
    }
}
