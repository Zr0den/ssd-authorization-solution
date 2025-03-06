using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ssd_authorization_solution;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("AppDb");
    options.UseSqlite(connectionString);
});
builder.Services.AddScoped<DbSeeder>();


builder.Services.AddIdentityApiEndpoints<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;

        options.Password.RequireDigit = true;          
        options.Password.RequireLowercase = true;      
        options.Password.RequireUppercase = true;      
        options.Password.RequireNonAlphanumeric = true; 
        options.Password.RequiredLength = 8;          

        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); 
        options.Lockout.MaxFailedAccessAttempts = 5; 
        options.Lockout.AllowedForNewUsers = true;   

        options.User.RequireUniqueEmail = true;      
    })
    .AddRoles<IdentityRole>()  // Enable role-based authorization
    .AddEntityFrameworkStores<AppDbContext>() // Use your database for identity storage
    .AddDefaultTokenProviders(); // Enables token generation for password reset, email confirmation, etc.

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EditorPolicy", policy => policy.RequireRole(Roles.Editor));
    options.AddPolicy("WriterPolicy", policy => policy.RequireRole(Roles.Writer));
    options.AddPolicy("SubscriberPolicy", policy => policy.RequireRole(Roles.Subscriber));
});


//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultScheme = IdentityConstants.ApplicationScheme;
//    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
//})
//.AddCookie(IdentityConstants.ApplicationScheme, options =>
//{
//    options.LoginPath = "/Account/Login";
//    options.AccessDeniedPath = "/Account/AccessDenied";
//})
//.AddJwtBearer(options =>
//{
//    options.Authority = builder.Configuration["Jwt:Issuer"];
//    options.Audience = builder.Configuration["Jwt:Audience"];
//    options.RequireHttpsMetadata = false;
//});


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/access-denied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Only send over HTTPS
    options.Cookie.SameSite = SameSiteMode.Strict; // Prevent CSRF attacks
    options.SlidingExpiration = true; // Extend session on activity
});

builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourSuperSecretKey")), // Replace with a secure key
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<DbSeeder>().SeedAsync().Wait();
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseExceptionHandler("/error");
app.UseHsts();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapIdentityApi<IdentityUser>();

app.Run();