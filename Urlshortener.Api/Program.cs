using UrlShortener.Api.Models;
using UrlShortener.Api.Services;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using AspNetCoreRateLimit;
using FluentValidation;
using UrlShortener.Api.Validators;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Register MongoDbSettings from appsettings.json
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

// Register MongoDbService as a singleton
builder.Services.AddSingleton<MongoDbService>();

// Register rate limiting services
builder.Services.AddOptions();
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

// Register FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<UserRegisterDtoValidator>();

// (Optional) Remove OpenAPI/Swagger if not needed
// builder.Services.AddOpenApi();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "JwtBearer";
    options.DefaultChallengeScheme = "JwtBearer";
})
.AddJwtBearer("JwtBearer", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("THIS_IS_A_DEMO_SECRET_CHANGE_ME_123456789")),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Add CORS support
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapPost("/register", async ([FromBody] UserRegisterDto dto, MongoDbService db, IValidator<UserRegisterDto> validator) =>
{
    var validation = await validator.ValidateAsync(dto);
    if (!validation.IsValid)
        return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));

    var existing = await db.Users.Find(u => u.Username == dto.Username).FirstOrDefaultAsync();
    if (existing != null)
        return Results.BadRequest(new { message = "Username already exists." });

    var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
    var user = new User { Username = dto.Username, PasswordHash = hash };
    await db.Users.InsertOneAsync(user);
    return Results.Ok(new { message = "Registration successful." });
});

app.MapPost("/login", async ([FromBody] UserLoginDto dto, MongoDbService db, IValidator<UserLoginDto> validator) =>
{
    try
    {
        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        var user = await db.Users.Find(u => u.Username == dto.Username).FirstOrDefaultAsync();
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Results.BadRequest(new { message = "Invalid username or password." });

        var token = GenerateJwtToken(user);
        return Results.Ok(new { token });
    }
    catch (Exception ex)
    {
        // Log the exception (in production, use proper logging)
        Console.WriteLine($"Login error: {ex.Message}");
        return Results.Problem("An error occurred during login. Please try again.");
    }
});

app.MapGet("/me", (ClaimsPrincipal user) =>
{
    if (!user.Identity.IsAuthenticated)
        return Results.Unauthorized();
    return Results.Ok(new
    {
        id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
        username = user.Identity.Name
    });
}).RequireAuthorization();

app.MapPost("/shorten", async (HttpContext http, [FromBody] ShortenRequestDto dto, MongoDbService db, IValidator<ShortenRequestDto> validator) =>
{
    var validation = await validator.ValidateAsync(dto);
    if (!validation.IsValid)
        return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));

    var userId = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userId == null)
        return Results.Unauthorized();

    // Generate unique short code
    string code;
    do {
        code = GenerateShortCode(6);
    } while (await db.ShortUrls.Find(s => s.ShortCode == code).AnyAsync());

    var shortUrl = new ShortUrl
    {
        OriginalUrl = dto.OriginalUrl,
        ShortCode = code,
        UserId = userId,
        CreatedAt = DateTime.UtcNow
    };
    await db.ShortUrls.InsertOneAsync(shortUrl);

    var host = $"{http.Request.Scheme}://{http.Request.Host}";
    return Results.Ok(new { shortUrl = $"{host}/r/{code}" });
}).RequireAuthorization();

app.MapGet("/r/{shortCode}", async (string shortCode, MongoDbService db, HttpContext http, IValidator<string> codeValidator) =>
{
    var validation = await codeValidator.ValidateAsync(shortCode);
    if (!validation.IsValid)
        return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));

    var shortUrl = await db.ShortUrls.Find(s => s.ShortCode == shortCode).FirstOrDefaultAsync();
    if (shortUrl == null)
        return Results.NotFound(new { message = "Short URL not found." });

    // Log click analytics
    var analytics = new ClickAnalytics
    {
        ShortUrlId = shortUrl.Id,
        Timestamp = DateTime.UtcNow,
        IpAddress = http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        UserAgent = http.Request.Headers["User-Agent"].ToString()
    };
    await db.ClickAnalytics.InsertOneAsync(analytics);

    return Results.Redirect(shortUrl.OriginalUrl);
});

app.MapGet("/analytics/{shortCode}", async (string shortCode, MongoDbService db, HttpContext http, IValidator<string> codeValidator) =>
{
    var validation = await codeValidator.ValidateAsync(shortCode);
    if (!validation.IsValid)
        return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));

    var userId = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userId == null)
        return Results.Unauthorized();

    var shortUrl = await db.ShortUrls.Find(s => s.ShortCode == shortCode && s.UserId == userId).FirstOrDefaultAsync();
    if (shortUrl == null)
        return Results.NotFound(new { message = "Short URL not found or not owned by user." });

    var analytics = await db.ClickAnalytics.Find(a => a.ShortUrlId == shortUrl.Id)
        .SortByDescending(a => a.Timestamp)
        .ToListAsync();

    return Results.Ok(analytics);
}).RequireAuthorization();

// Dashboard endpoints
app.MapGet("/dashboard", async (HttpContext http, MongoDbService db) =>
{
    var userId = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userId == null)
        return Results.Unauthorized();

    var host = $"{http.Request.Scheme}://{http.Request.Host}";
    var now = DateTime.UtcNow;
    var today = now.Date;
    var weekAgo = today.AddDays(-7);
    var monthAgo = today.AddDays(-30);

    // Get all user's links
    var userLinks = await db.ShortUrls.Find(s => s.UserId == userId).ToListAsync();
    
    var dashboard = new DashboardSummaryDto
    {
        TotalLinks = userLinks.Count,
        TopLinks = new List<LinkSummaryDto>()
    };

    var totalClicks = 0;
    var clicksToday = 0;
    var clicksThisWeek = 0;
    var clicksThisMonth = 0;

    // Calculate analytics for each link
    foreach (var link in userLinks)
    {
        var linkClicks = await db.ClickAnalytics.Find(a => a.ShortUrlId == link.Id).ToListAsync();
        var linkClicksToday = linkClicks.Count(c => c.Timestamp.Date == today);
        var linkClicksThisWeek = linkClicks.Count(c => c.Timestamp.Date >= weekAgo);
        var linkClicksThisMonth = linkClicks.Count(c => c.Timestamp.Date >= monthAgo);

        totalClicks += linkClicks.Count;
        clicksToday += linkClicksToday;
        clicksThisWeek += linkClicksThisWeek;
        clicksThisMonth += linkClicksThisMonth;

        dashboard.TopLinks.Add(new LinkSummaryDto
        {
            Id = link.Id,
            ShortCode = link.ShortCode,
            OriginalUrl = link.OriginalUrl,
            ShortUrl = $"{host}/r/{link.ShortCode}",
            CreatedAt = link.CreatedAt,
            TotalClicks = linkClicks.Count,
            ClicksToday = linkClicksToday,
            ClicksThisWeek = linkClicksThisWeek,
            ClicksThisMonth = linkClicksThisMonth
        });
    }

    // Sort by total clicks descending
    dashboard.TopLinks = dashboard.TopLinks.OrderByDescending(l => l.TotalClicks).Take(10).ToList();
    dashboard.TotalClicks = totalClicks;
    dashboard.ClicksToday = clicksToday;
    dashboard.ClicksThisWeek = clicksThisWeek;
    dashboard.ClicksThisMonth = clicksThisMonth;

    // Get daily clicks for the last 30 days
    var dailyClicks = new List<DailyClicksDto>();
    for (int i = 29; i >= 0; i--)
    {
        var date = today.AddDays(-i);
        var dayClicks = 0;
        
        foreach (var link in userLinks)
        {
            var linkClicks = await db.ClickAnalytics.Find(a => a.ShortUrlId == link.Id && a.Timestamp.Date == date).CountDocumentsAsync();
            dayClicks += (int)linkClicks;
        }
        
        dailyClicks.Add(new DailyClicksDto { Date = date, Clicks = dayClicks });
    }
    dashboard.DailyClicks = dailyClicks;

    return Results.Ok(dashboard);
}).RequireAuthorization();

app.MapGet("/dashboard/links", async (HttpContext http, MongoDbService db) =>
{
    var userId = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userId == null)
        return Results.Unauthorized();

    var host = $"{http.Request.Scheme}://{http.Request.Host}";
    var now = DateTime.UtcNow;
    var today = now.Date;
    var weekAgo = today.AddDays(-7);
    var monthAgo = today.AddDays(-30);

    var userLinks = await db.ShortUrls.Find(s => s.UserId == userId)
        .SortByDescending(s => s.CreatedAt)
        .ToListAsync();

    var linksWithAnalytics = new List<LinkSummaryDto>();

    foreach (var link in userLinks)
    {
        var linkClicks = await db.ClickAnalytics.Find(a => a.ShortUrlId == link.Id).ToListAsync();
        var linkClicksToday = linkClicks.Count(c => c.Timestamp.Date == today);
        var linkClicksThisWeek = linkClicks.Count(c => c.Timestamp.Date >= weekAgo);
        var linkClicksThisMonth = linkClicks.Count(c => c.Timestamp.Date >= monthAgo);

        linksWithAnalytics.Add(new LinkSummaryDto
        {
            Id = link.Id,
            ShortCode = link.ShortCode,
            OriginalUrl = link.OriginalUrl,
            ShortUrl = $"{host}/r/{link.ShortCode}",
            CreatedAt = link.CreatedAt,
            TotalClicks = linkClicks.Count,
            ClicksToday = linkClicksToday,
            ClicksThisWeek = linkClicksThisWeek,
            ClicksThisMonth = linkClicksThisMonth
        });
    }

    return Results.Ok(linksWithAnalytics);
}).RequireAuthorization();

app.MapGet("/dashboard/links/{shortCode}", async (string shortCode, HttpContext http, MongoDbService db, IValidator<string> codeValidator) =>
{
    var validation = await codeValidator.ValidateAsync(shortCode);
    if (!validation.IsValid)
        return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));

    var userId = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userId == null)
        return Results.Unauthorized();

    var shortUrl = await db.ShortUrls.Find(s => s.ShortCode == shortCode && s.UserId == userId).FirstOrDefaultAsync();
    if (shortUrl == null)
        return Results.NotFound(new { message = "Short URL not found or not owned by user." });

    var host = $"{http.Request.Scheme}://{http.Request.Host}";
    var now = DateTime.UtcNow;
    var today = now.Date;

    var clicks = await db.ClickAnalytics.Find(a => a.ShortUrlId == shortUrl.Id)
        .SortByDescending(a => a.Timestamp)
        .ToListAsync();

    // Get daily clicks for the last 30 days
    var dailyClicks = new List<DailyClicksDto>();
    for (int i = 29; i >= 0; i--)
    {
        var date = today.AddDays(-i);
        var dayClicks = clicks.Count(c => c.Timestamp.Date == date);
        dailyClicks.Add(new DailyClicksDto { Date = date, Clicks = dayClicks });
    }

    var linkAnalytics = new LinkAnalyticsDto
    {
        Id = shortUrl.Id,
        ShortCode = shortUrl.ShortCode,
        OriginalUrl = shortUrl.OriginalUrl,
        ShortUrl = $"{host}/r/{shortCode}",
        CreatedAt = shortUrl.CreatedAt,
        TotalClicks = clicks.Count,
        Clicks = clicks,
        DailyClicks = dailyClicks
    };

    return Results.Ok(linkAnalytics);
}).RequireAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

string GenerateJwtToken(User user)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes("THIS_IS_A_DEMO_SECRET_CHANGE_ME_123456789"); // TODO: Move to config
    
    // Ensure user has an ID for the token
    if (string.IsNullOrEmpty(user.Id))
    {
        throw new InvalidOperationException("User ID is required for token generation");
    }
    
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Username)
        }),
        Expires = DateTime.UtcNow.AddDays(7),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}

string GenerateShortCode(int length)
{
    const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    var random = new Random();
    return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
}

app.UseAuthentication();
app.UseAuthorization();
app.UseIpRateLimiting();

app.Run();
