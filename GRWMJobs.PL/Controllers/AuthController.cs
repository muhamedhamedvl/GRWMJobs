using GRWMJobs.DAL.Data;
using GRWMJobs.DAL.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Data;
using System.Security.Claims;

namespace GRWMJobs.PL.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private const int MaxAttempts = 5;
        private static readonly TimeSpan LockoutWindow = TimeSpan.FromMinutes(10);

        public AuthController(AppDbContext context, IMemoryCache cache, IConfiguration configuration)
        {
            _context = context;
            _cache = cache;
            _configuration = configuration;
        }

        private string GetAttemptKey(string username, string ip)
        {
            username = (username ?? string.Empty).Trim().ToLowerInvariant();
            ip = ip ?? string.Empty;
            return $"login:attempts:{username}:{ip}";
        }
        private string GetLockKey(string username, string ip)
        {
            username = (username ?? string.Empty).Trim().ToLowerInvariant();
            ip = ip ?? string.Empty;
            return $"login:lock:{username}:{ip}";
        }

        [HttpGet]
        public IActionResult Login()
        {
            EnsureAdminUser();
            return View();
        }

        private void EnsureAdminUser()
        {
            var adminUsername = _configuration["Admin:Username"];
            var adminEmail = _configuration["Admin:Email"];
            var adminPassword = _configuration["Admin:Password"];

            if (!string.IsNullOrEmpty(adminUsername) && !string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword))
            {
                var admin = _context.Users.FirstOrDefault(u => u.Name == adminUsername);
                if (admin == null)
                {
                    admin = new User
                    {
                        Name = adminUsername,
                        Email = adminEmail,
                        Password = adminPassword,
                        Role = Role.Admin
                    };
                    _context.Users.Add(admin);
                    _context.SaveChanges();
                }
                else if (admin.Password != adminPassword)
                {
                    admin.Password = adminPassword;
                    _context.SaveChanges();
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Username and password are required");
                return View();
            }

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var lockKey = GetLockKey(username, ip ?? "");
            if (_cache.TryGetValue<DateTime>(lockKey, out var lockUntil) && lockUntil > DateTime.UtcNow)
            {
                ModelState.AddModelError("", $"Account is locked until {lockUntil:HH:mm:ss} UTC");
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == username);
            if (user == null || user.Password != password)
            {
                RegisterFailedAttempt(username, ip, out var warning);
                if (!string.IsNullOrEmpty(warning)) ModelState.AddModelError("", warning);
                else ModelState.AddModelError("", "Invalid credentials");
                return View();
            }

            ResetAttempts(username, ip);

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(12)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToAction("Index", "Home");
        }

        private void RegisterFailedAttempt(string username, string? ip, out string? warning)
        {
            warning = null;
            var attemptKey = GetAttemptKey(username, ip ?? "");
            int attempts = 0;
            if (_cache.TryGetValue<int>(attemptKey, out var existing)) attempts = existing;
            attempts++;
            var opts = new MemoryCacheEntryOptions().SetAbsoluteExpiration(LockoutWindow);
            _cache.Set(attemptKey, attempts, opts);

            if (attempts >= MaxAttempts)
            {
                var lockKey = GetLockKey(username, ip ?? "");
                _cache.Set(lockKey, DateTime.UtcNow.Add(LockoutWindow), new MemoryCacheEntryOptions().SetAbsoluteExpiration(LockoutWindow));
                warning = $"Too many attempts. Your account is locked for {(int)LockoutWindow.TotalMinutes} minutes.";
            }
            else if (attempts == MaxAttempts - 1)
            {
                warning = "Warning: only 1 attempt left before a 10-minute lock.";
            }
        }

        private void ResetAttempts(string username, string? ip)
        {
            var attemptKey = GetAttemptKey(username, ip ?? "");
            _cache.Remove(attemptKey);
            var lockKey = GetLockKey(username, ip ?? "");
            _cache.Remove(lockKey);
        }

        [HttpGet]
        public IActionResult Register()
        {
            var code = GenerateHumanCode();
            HttpContext.Session.SetString("reg:code", code);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string name, string email, string password, string confirmPassword, string verificationCode)
        {
            var expected = HttpContext.Session.GetString("reg:code");
            if (string.IsNullOrWhiteSpace(verificationCode) || string.IsNullOrWhiteSpace(expected) || !string.Equals(verificationCode.Trim(), expected, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Invalid verification code.");
                HttpContext.Session.SetString("reg:code", GenerateHumanCode());
                return View();
            }

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ModelState.AddModelError("", "Username, Email, Password, and Confirm Password are required");
                HttpContext.Session.SetString("reg:code", GenerateHumanCode());
                return View();
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match");
                HttpContext.Session.SetString("reg:code", GenerateHumanCode());
                return View();
            }
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Name == name);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "Username already taken");
                HttpContext.Session.SetString("reg:code", GenerateHumanCode());
                return View();
            }
            var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingEmail != null)
            {
                ModelState.AddModelError("", "Email already registered");
                HttpContext.Session.SetString("reg:code", GenerateHumanCode());
                return View();
            }

            // Create new user
            var user = new User
            {
                Name = name,
                Email = email,
                Password = password,
                Role = Role.User
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Registration successful! You can now log in.";
            return RedirectToAction("Login");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult RegistrationCode()
        {
            var code = GenerateHumanCode();
            HttpContext.Session.SetString("reg:code", code);

            var rnd = new Random();
            var lines = string.Join("", Enumerable.Range(0, 6).Select(_ =>
                $"<line x1='{rnd.Next(0, 120)}' y1='{rnd.Next(0, 40)}' " +
                $"x2='{rnd.Next(0, 120)}' y2='{rnd.Next(0, 40)}' " +
                $"stroke='rgba(0,0,0,0.{rnd.Next(2, 6)})' " +
                $"stroke-width='{rnd.Next(1, 3)}'/>"));

            var chars = string.Join("", code.ToUpperInvariant().Select((c, i) =>
            {
                var x = 10 + i * 20 + rnd.Next(-3, 3);
                var y = 25 + rnd.Next(-5, 5);
                var rotate = rnd.Next(-25, 25);
                var color = $"rgb({rnd.Next(0, 180)}, {rnd.Next(0, 180)}, {rnd.Next(0, 180)})";
                return $"<text x='{x}' y='{y}' fill='{color}' font-size='{rnd.Next(18, 24)}' " +
                       $"transform='rotate({rotate} {x} {y})' " +
                       "font-family='Consolas, monospace'>" +
                       $"{c}</text>";
            }));

            var svg = "<svg xmlns='http://www.w3.org/2000/svg' width='120' height='40' viewBox='0 0 120 40'>" +
                      "<rect width='120' height='40' fill='#f8fafc'/>" + lines + chars + "</svg>";

            return Content(svg, "image/svg+xml");
        }
        private static string GenerateHumanCode()
        {
            const string alphabet = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";
            var rnd = new Random();
            return new string(Enumerable.Range(0, 5).Select(_ => alphabet[rnd.Next(alphabet.Length)]).ToArray());
        }
    }
}