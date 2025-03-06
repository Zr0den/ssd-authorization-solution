using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ssd_authorization_solution.DTOs;
using ssd_authorization_solution.Entities;

namespace MyApp.Namespace;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class CommentController : ControllerBase
{
    private readonly AppDbContext db;

    public CommentController(AppDbContext ctx)
    {
        this.db = ctx;
    }

    [HttpGet]
    public IEnumerable<CommentDto> Get([FromQuery] int? articleId)
    {
        var query = db.Comments.Include(x => x.Author).AsQueryable();
        if (articleId.HasValue)
            query = query.Where(c => c.ArticleId == articleId);
        return query.Select(CommentDto.FromEntity);
    }

    [HttpGet(":id")]
    public CommentDto? GetById(int id)
    {
        return db
            .Comments.Include(x => x.Author)
            .Select(CommentDto.FromEntity)
            .SingleOrDefault(x => x.Id == id);
    }

    [HttpPost]
    [Authorize(Policy = "SubscriberPolicy")]
    public CommentDto Post([FromBody] CommentFormDto dto)
    {
        var userName = HttpContext.User.Identity?.Name;
        var author = db.Users.Single(x => x.UserName == userName);
        var article = db.Articles.Single(x => x.Id == dto.ArticleId);
        var entity = new Comment
        {
            Content = dto.Content,
            Article = article,
            Author = author,
        };
        var created = db.Comments.Add(entity).Entity;
        db.SaveChanges();
        return CommentDto.FromEntity(created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Subscriber,Editor")]
    public IActionResult Put(int id, [FromBody] CommentFormDto dto)
    {
        var userName = HttpContext.User.Identity?.Name;
        var entity = db
            .Comments.Include(x => x.Author)
            .SingleOrDefault(x => x.Id == id);

        if (entity == null)
        {
            return NotFound();
        }

        if (entity.Author.UserName != userName && !User.IsInRole(Roles.Editor))
        {
            return Forbid();
        }

        entity.Content = dto.Content;
        db.Comments.Update(entity);
        db.SaveChanges();
        return Ok(CommentDto.FromEntity(entity));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "EditorPolicy")]
    public IActionResult Delete(int id)
    {
        var entity = db.Comments.SingleOrDefault(x => x.Id == id);
        if (entity == null)
        {
            return NotFound();
        }

        db.Comments.Remove(entity);
        db.SaveChanges();
        return NoContent();
    }
}
