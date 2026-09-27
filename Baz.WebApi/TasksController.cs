using Baz.UseCases.Exceptions;
using Baz.UseCases.Tasks;
using Baz.UseCases.Tasks.Delete;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Baz.WebApi;

[Route("/api/[controller]")]
[ApiController]
public class TasksController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ISender sender, ILogger<TasksController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateTaskRequest request, CancellationToken ct)
    {
        try
        {
            var response = await _sender.Send(request, ct);
            return Ok(response.Task);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Ошибка валидации: {Msg}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, "Непредвиденная ошибка: {Msg}", ex.Message);
            throw;
        }
    }

    [HttpPut]
    [Route("{id:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, UpdateTaskRequest request, CancellationToken ct)
    {
        request = request with { Id = id};

        try
        {
            var response = await _sender.Send(request, ct);
            return Ok(response.Task);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Ошибка валидации: {Msg}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, "Непредвиденная ошибка: {Msg}", ex.Message);
            throw;
        }
    }

    [HttpDelete]
    [Route("{id:int}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] int id, CancellationToken ct)
    {
        try
        {
            var response = await _sender.Send(new DeleteTaskRequest(id), ct);
            return Ok();
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Ошибка валидации: {Msg}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, "Непредвиденная ошибка: {Msg}", ex.Message);
            throw;
        }
    }

    [HttpGet]
    [Route("{id:int}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] int id, CancellationToken ct)
    {
        try
        {
            var response = await _sender.Send(new GetTaskRequest(id), ct);
            return Ok(response.Tasks);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Ошибка валидации: {Msg}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, "Непредвиденная ошибка: {Msg}", ex.Message);
            throw;
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync(CancellationToken ct)
    {
        try
        {
            var response = await _sender.Send(new GetTaskRequest(), ct);
            return Ok(response.Tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, "Непредвиденная ошибка: {Msg}", ex.Message);
            throw;
        }
    }
}
