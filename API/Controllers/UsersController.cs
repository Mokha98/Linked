using API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController:ControllerBase
{
    private readonly DataContext _dbContext;
    public UsersController(DataContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
    {
        var users = await _dbContext.AppUsers.ToArrayAsync();
        return users;
    }

   [HttpGet("{id}")]
    public async Task<ActionResult<AppUser>> GetUser(int id)
    {
        var users = await _dbContext.AppUsers.FindAsync(id);
        return users;
    }

}
