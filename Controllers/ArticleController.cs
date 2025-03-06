using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ssd_authorization_solution.DTOs;
using ssd_authorization_solution.Entities;

namespace MyApp.Namespace;

[Route("api/[controller]")]
[ApiController]
public class ArticleController : ControllerBase
{
    private readonly AppDbContext db;

    public ArticleController(AppDbContext ctx)
    {
        this.db = ctx;
    }

    [HttpGet]
    public IEnumerable<ArticleDto> Get()
    {
        return db.Articles.Include(x => x.Author).Select(ArticleDto.FromEntity);
    }

    [HttpGet(":id")]
    public ArticleDto? GetById(int id)
    {
        return db
            .Articles.Include(x => x.Author)
            .Where(x => x.Id == id)
            .Select(ArticleDto.FromEntity)
            .SingleOrDefault();
    }

    [HttpPost]
    [Authorize(Policy = "WriterPolicy")]
    public ArticleDto Post([FromBody] ArticleFormDto dto)
    {
        var userName = HttpContext.User.Identity?.Name;
        var author = db.Users.Single(x => x.UserName == userName);
        var entity = new Article
        {
            Title = dto.Title,
            Content = dto.Content,
            Author = author,
            CreatedAt = DateTime.Now
        };
        var created = db.Articles.Add(entity).Entity;
        db.SaveChanges();
        return ArticleDto.FromEntity(created);
    }

    [HttpPut(":id")]
    [Authorize(Policy = "WriterPolicy")]
    public ArticleDto Put(int id, [FromBody] ArticleFormDto dto)
    {
        var userName = HttpContext.User.Identity?.Name;
        var entity = db
            .Articles
            .Include(x => x.Author)
            .Single(x => x.Id == id);
        entity.Title = dto.Title;
        entity.Content = dto.Content;
        var updated = db.Articles.Update(entity).Entity;
        db.SaveChanges();
        return ArticleDto.FromEntity(updated);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "EditorPolicy")]
    public IActionResult Delete(int id)
    {
        var entity = db.Articles.SingleOrDefault(x => x.Id == id);
        if (entity == null)
        {
            return NotFound();
        }

        db.Articles.Remove(entity);
        db.SaveChanges();
        return NoContent();
    }
}
