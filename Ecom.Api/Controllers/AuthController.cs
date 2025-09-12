using BCrypt.Net;
using Ecom.Api.Services;
using Ecom.Domain.Entities;
using Ecom.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecom.Api.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly EcomDbContext _db;
        private readonly JwtTokenService _jwt;
        public AuthController(EcomDbContext db, JwtTokenService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        public record LoginRequest(string Username, string Password);
        public record RegisterRequest(string Username, string Password, string Role);

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
            if (user is null || !user.IsActive)
                return Unauthorized("Kullanıcı bulunamadı veya pasif.");

            // Parola doğrulama
            var ok = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
            if (!ok) return Unauthorized("Kullanıcı adı veya şifre hatalı.");

            user.LastLoginAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var (token, expires) = _jwt.Create(user.Id.ToString(), user.Username, user.Role);
            return Ok(new
            {
                accessToken = token,
                tokenType = "Bearer",
                expiresAtUtc = expires,
                user = new { user.Id, user.Username, user.Role }
            });
        }

        // İstersen başlangıçta admin eklemek için açık bırak;
        // prod'da kapat ya da sadece development'a izin ver.
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            var exists = await _db.Users.AnyAsync(u => u.Username == req.Username);
            if (exists) return Conflict("Bu kullanıcı adı zaten var.");

            var entity = new User
            {
                Id = Guid.NewGuid(),
                Username = req.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password), // salt + workfactor içerir
                Role = string.IsNullOrWhiteSpace(req.Role) ? "Admin" : req.Role,
                IsActive = true
            };

            _db.Users.Add(entity);
            await _db.SaveChangesAsync();

            return Created("", new { entity.Id, entity.Username, entity.Role });
        }
    }
}
