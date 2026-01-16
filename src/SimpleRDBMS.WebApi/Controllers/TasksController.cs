
using Microsoft.AspNetCore.Mvc;
using SimpleRDBMS.Application.Commands;
using SimpleRDBMS.Application.Interfaces;
using SimpleRDBMS.Application.Queries;
using SimpleRDBMS.Domain.Entities;
using SimpleRDBMS.Infrastructure.Persistence.Interfaces;

namespace SimpleRDBMS.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly IQueryExecutor _queryExecutor;
    private readonly ICommandExecutor _commandExecutor;
    private readonly ITableRepository _tableRepository;
    private static int _nextId = 1;

    public TasksController(
        IQueryExecutor queryExecutor,
        ICommandExecutor commandExecutor,
        ITableRepository tableRepository)
    {
        _queryExecutor = queryExecutor;
        _commandExecutor = commandExecutor;
        _tableRepository = tableRepository;
    }

    // GET: api/tasks
    [HttpGet]
    public IActionResult GetAll([FromQuery] bool? completed = null)
    {
        try
        {
            string? whereClause = completed.HasValue 
                ? $"completed = {(completed.Value ? 1 : 0)}" 
                : null;

            var query = new SelectQuery(
                new List<string> { "*" },
                "tasks",
                whereClause ?? "" // Empty string instead of null
            );

            var result = _queryExecutor.Execute(query);
            
            var tasks = result.Rows.Select(r => new
            {
                id = r.Values["id"],
                title = r.Values["title"],
                description = r.Values["description"],
                completed = Convert.ToInt32(r.Values["completed"]) == 1,
                priority = r.Values["priority"]
            });

            return Ok(tasks);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // GET: api/tasks/5
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        try
        {
            var query = new SelectQuery(
                new List<string> { "*" },
                "tasks",
                $"id = {id}"
            );

            var result = _queryExecutor.Execute(query);
            
            if (result.Rows.Count == 0)
                return NotFound(new { error = "Task not found" });

            var task = result.Rows[0];
            return Ok(new
            {
                id = task.Values["id"],
                title = task.Values["title"],
                description = task.Values["description"],
                completed = Convert.ToInt32(task.Values["completed"]) == 1,
                priority = task.Values["priority"]
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // POST: api/tasks
    [HttpPost]
    public IActionResult Create([FromBody] CreateTaskRequest request)
    {
        try
        {
            var id = _nextId++;
            var rowData = new Dictionary<string, object>
            {
                { "id", id },
                { "title", request.Title },
                { "description", request.Description ?? "" },
                { "completed", 0 },
                { "priority", request.Priority ?? 2 }
            };

            var command = new InsertCommand(
                _tableRepository,
                "tasks", 
                new Row(rowData)
            );
            
            var result = _commandExecutor.Execute(command);

            if (result.Success)
                return CreatedAtAction(nameof(GetById), new { id }, new 
                { 
                    id, 
                    title = request.Title,
                    description = request.Description,
                    completed = false,
                    priority = request.Priority ?? 2
                });

            return BadRequest(new { error = result.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // PUT: api/tasks/5
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] UpdateTaskRequest request)
    {
        try
        {
            var setValues = new Dictionary<string, object>();
            
            if (request.Title != null)
                setValues["title"] = request.Title;
            if (request.Description != null)
                setValues["description"] = request.Description;
            if (request.Completed.HasValue)
                setValues["completed"] = request.Completed.Value ? 1 : 0;
            if (request.Priority.HasValue)
                setValues["priority"] = request.Priority.Value;

            if (setValues.Count == 0)
                return BadRequest(new { error = "No fields to update" });

            var command = new UpdateCommand("tasks", setValues, $"id = {id}");
            var result = _commandExecutor.Execute(command);

            if (result.Success && result.RowsAffected > 0)
                return Ok(new { message = result.Message, rowsAffected = result.RowsAffected });

            return NotFound(new { error = "Task not found" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // DELETE: api/tasks/5
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        try
        {
            var command = new DeleteCommand("tasks", $"id = {id}");
            var result = _commandExecutor.Execute(command);

            if (result.Success && result.RowsAffected > 0)
                return Ok(new { message = result.Message, rowsAffected = result.RowsAffected });

            return NotFound(new { error = "Task not found" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // GET: api/tasks/stats
    [HttpGet("stats")]
    public IActionResult GetStats()
    {
        try
        {
            var allQuery = new SelectQuery(new List<string> { "*" }, "tasks", "");
            var allTasks = _queryExecutor.Execute(allQuery);

            var completedQuery = new SelectQuery(new List<string> { "*" }, "tasks", "completed = 1");
            var completedTasks = _queryExecutor.Execute(completedQuery);

            return Ok(new
            {
                total = allTasks.Rows.Count,
                completed = completedTasks.Rows.Count,
                pending = allTasks.Rows.Count - completedTasks.Rows.Count
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class CreateTaskRequest
{
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int? Priority { get; set; }
}

public class UpdateTaskRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool? Completed { get; set; }
    public int? Priority { get; set; }
}
