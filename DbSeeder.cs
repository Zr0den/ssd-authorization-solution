using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ssd_authorization_solution.Entities;

namespace ssd_authorization_solution;

public class DbSeeder
{
    private readonly AppDbContext ctx;
    private readonly ILogger<DbSeeder> logger;
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly UserManager<IdentityUser> userManager;

    public DbSeeder(
        ILogger<DbSeeder> logger,
        AppDbContext ctx,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager
    )
    {
        this.logger = logger;
        this.ctx = ctx;
        this.userManager = userManager;
        this.roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        await ctx.Database.MigrateAsync();

        await EnsureRoleExists(Roles.Editor);
        await EnsureRoleExists(Roles.Writer);
        await EnsureRoleExists(Roles.Subscriber);

        var editor = await CreateUser("bobby123@gmail.com", "S3cret!123", Roles.Editor);
        var writer = await CreateUser("egon@gmail.com", "S3cret!123", Roles.Writer);
        var anotherWriter = await CreateUser("mulvarp@gmail.com", "S3cret!123", Roles.Writer);
        var subscriber = await CreateUser("fupmager@gmail.com", "S3cret!123", Roles.Subscriber);

        var article1 = ctx.Articles.Add(
            new Article
            {
                Title = "First article",
                Content = "Breaking news",
                Author = writer
            }
        ).Entity;

        var article2 = ctx.Articles.Add(
            new Article
            {
                Title = "Second article",
                Content = "Another breaking news",
                Author = anotherWriter
            }
        ).Entity;

        ctx.Comments.Add(
            new Comment
            {
                Content = "First comment",
                Author = subscriber,
                Article = article1
            }
        );

        ctx.Comments.Add(
            new Comment
            {
                Content = "I'm a troll",
                Author = subscriber,
                Article = article2
            }
        );

        await ctx.SaveChangesAsync();
    }

    private async Task EnsureRoleExists(string roleName)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role == null)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    private async Task<IdentityUser> CreateUser(string username, string password, string role)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }

        var user = new IdentityUser
        {
            UserName = username,
            Email = $"{username}@example.com",
            EmailConfirmed = true
        };

        var existingUser = await userManager.FindByNameAsync(username);
        if (existingUser == null)
        {
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    logger.LogWarning("{Code}: {Description}", error.Code, error.Description);
            }
            else
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }

        return await userManager.FindByNameAsync(username);
    }
}