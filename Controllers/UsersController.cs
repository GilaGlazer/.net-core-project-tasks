using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webApiProject.Models;
using webApiProject.Services;

namespace webApiProject.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly UsersServiceJson usersService;

    public UsersController(UsersServiceJson usersService)
    {
        this.usersService = usersService;
    }

    [HttpGet]
    [Authorize(Policy = "admin")]
    public ActionResult<IEnumerable<Users>> Get()
    {
        return usersService.Get();
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "user")]
    public ActionResult<Users> Get(int id)
    {
        var user = usersService.GetMyUser();
        if (user == null)
            return NotFound();
        return user;
    }

    [HttpPost]
    [Authorize(Policy = "admin")]
    public ActionResult Post(Users newItem)
    {
        var newId = usersService.Insert(newItem);
        if (newId == -1)
            return BadRequest();
        return CreatedAtAction(nameof(Post), new { Id = newId }, newItem);
    }

    [HttpPut("{id}")]
    public ActionResult Put(int id, Users newItem)
    {
        if (usersService.Update(id, newItem))
            return NoContent();
        return BadRequest();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "admin")]
    public ActionResult Delete(int id)
    {
        if (usersService.Delete(id))
            return Ok();
        return NotFound();
    }
}
