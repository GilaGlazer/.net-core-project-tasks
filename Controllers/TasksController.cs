using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webApiProject.Interfaces;
using webApiProject.Models;

namespace webApiProject.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Policy = "user")]
public class TasksController : ControllerBase
{
    private readonly IService<Tasks> TasksService;

    public TasksController(IService<Tasks> TasksService)
    {
        this.TasksService = TasksService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Tasks>> Get()
    {
        return TasksService.Get();
    }

    [HttpGet("{id}")]
    public ActionResult<Tasks> Get(int id)
    {
        var task = TasksService.Get(id);
        if (task == null)
            return NotFound();
        return task;
    }

    [HttpPost]
    public ActionResult Post(Tasks newTask)
    {
        var newId = TasksService.Insert(newTask);
        if (newId == -1)
            return BadRequest();
        return CreatedAtAction(nameof(Post), new { Id = newId });
    }

    [HttpPut("{id}")]
    public ActionResult Put(int id, Tasks updatedTask)
    {
        if (TasksService.Update(id, updatedTask))
            return NoContent();
        return BadRequest();
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        if (TasksService.Delete(id))
            return Ok();
        return NotFound();
    }
}
