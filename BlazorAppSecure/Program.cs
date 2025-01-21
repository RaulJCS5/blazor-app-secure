using BlazorAppSecure;
using BlazorAppSecure.Data;
using BlazorAppSecure.Database;
using BlazorAppSecure.Extensions;
using BlazorAppSecure.Handlers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddHttpClient(); // Add this line

builder.Services.AddTransient<HttpClientHandler>(); // Register HttpClientHandler as transient
builder.Services.AddScoped<UnauthorizedResponseHandler>(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    var innerHandler = sp.GetRequiredService<HttpClientHandler>();
    return new UnauthorizedResponseHandler(navigationManager, innerHandler);
});
builder.Services.AddScoped(sp =>
{
    var logger = sp.GetRequiredService<ILogger<Program>>();
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    var handler = sp.GetRequiredService<UnauthorizedResponseHandler>();
    var httpClient = new HttpClient(handler) { BaseAddress = new Uri(navigationManager.BaseUri) };

    logger.LogInformation("NavigationManager: {NavigationManager}", navigationManager);
    logger.LogInformation("UnauthorizedResponseHandler: {Handler}", handler);
    logger.LogInformation("HttpClient: {HttpClient}", httpClient);

    return httpClient;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BlazorAppSecure API", Version = "v1" });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddIdentityCookies();

//builder.Services.AddAuthorization();

var connectionString = builder.Configuration.GetConnectionString("Database") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentityCore<User>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<User>, IdentityNoOpEmailSender>();

//builder.Services.AddAntiforgery(options =>
//{
//    options.HeaderName = "X-CSRF-TOKEN";
//});

var app = builder.Build();

// Seed initial data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<User>>();
    context.Database.Migrate();
    SeedData(context, userManager).Wait();
}

async Task SeedData(ApplicationDbContext context, UserManager<User> userManager)
{
    if (context.Users.Any())
    {
        var user = new User
        {
            UserName = "testuser",
            Email = "testuser@example.com",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(user, "Password123!");
    }
}

// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    //app.UseExceptionHandler("/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    //app.UseHsts();
//    app.UseSwagger();
//    app.UseSwaggerUI();

//    app.ApplyMigrations();
//}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

//app.UseAuthentication();
//app.UseAuthorization();

//app.UseAntiforgery();

app.UseMiddleware<BlazorCookieLoginMiddleware>();

// Enable middleware to serve generated Swagger as a JSON endpoint and Swagger UI only in development.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BlazorAppSecure API V1");
        c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
    });

    app.ApplyMigrations();
}

app.MapGet("users/me", async (ClaimsPrincipal claims, ApplicationDbContext context) =>
{
    string userId = claims.Claims.First(claims => claims.Type == ClaimTypes.NameIdentifier).Value;
    var user = await context.Users.FindAsync(userId);
    return user;
}).RequireAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapIdentityApi<User>();

// Add additional endpoints required by the Identity /Account Razor components.
//app.MapAdditionalIdentityEndpoints();

app.Run();