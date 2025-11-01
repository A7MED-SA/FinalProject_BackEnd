using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using backend_project.Configuration;
using backend_project.DTOs.Auth;
using backend_project.Models;
using backend_project.Data;
using Microsoft.EntityFrameworkCore;

namespace backend_project.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly JwtSettings _jwtSettings;
    private readonly ApplicationDbContext _context;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IOptions<JwtSettings> jwtSettings,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtSettings = jwtSettings.Value;
        _context = context;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("USER_EXISTS: User with this email already exists");
        }

        // Create new user
        var user = new User
        {
            Name = registerDto.Name,
            Email = registerDto.Email,
            UserName = registerDto.Email, // Use email as username
            PhoneNumber = registerDto.PhoneNumber,
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow,
            
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => e.Description);
            var errorMessage = string.Join("; ", errors.Select(kv => $"{kv.Key}: {kv.Value}"));
            throw new ArgumentException($"REGISTRATION_FAILED: {errorMessage}");
        }

        // Assign default role (Student)
        var roleResult = await _userManager.AddToRoleAsync(user, "Student");
        if (!roleResult.Succeeded)
        {
            // Log warning but don't fail registration
            var roleErrors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
            // Consider using ILogger here: _logger.LogWarning("Failed to assign Student role: {Errors}", roleErrors);
        }

        // Generate tokens
        return await GenerateAuthResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        // Find user by email
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            throw new Exception("Invalid email or password");
        }

        // Check password
        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                throw new Exception("Account is locked out");
            }
            throw new Exception("Invalid email or password");
        }

        // Update last login
        user.LastLogin = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Generate tokens
        return await GenerateAuthResponse(user);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        // Find session with this refresh token
        var session = await _context.Sessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == HashToken(refreshToken) && s.ExpiresAt > DateTime.UtcNow);

        if (session == null)
        {
            throw new Exception("Invalid or expired refresh token");
        }

        // Generate new tokens
        return await GenerateAuthResponse(session.User);
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        var session = await _context.Sessions
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == HashToken(refreshToken));

        if (session == null)
        {
            return false;
        }

        _context.Sessions.Remove(session);
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<AuthResponseDto> GenerateAuthResponse(User user)
    {
        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);

        // Generate access token
        var accessToken = GenerateAccessToken(user, roles.ToList());

        // Generate refresh token
        var refreshToken = GenerateRefreshToken();
        var refreshTokenHash = HashToken(refreshToken);

        // Save session
        var session = new Session
        {
            UserId = user.Id,
            TokenHash = HashToken(accessToken),
            RefreshTokenHash = refreshTokenHash,
            IpAddress = null, // Set from HttpContext in controller
            UserAgent = null, // Set from HttpContext in controller
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow
        };

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            User = new UserInfoDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email ?? string.Empty,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Roles = roles.ToList()
            }
        };
    }

    private string GenerateAccessToken(User user, List<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add roles as claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
