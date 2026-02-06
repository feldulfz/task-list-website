using Microsoft.AspNetCore.Mvc;
using TaskManager.Models;
using task_manager_api.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using task_manager_api.Interfaces;

namespace task_manager_api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("tasks")]
    public class TasksController(ITaskRepository taskRepository) : ControllerBase
    {
        private readonly ITaskRepository _taskRepository = taskRepository;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tasks = await _taskRepository.GetAllTasksAsync();
            return Ok(tasks);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var task = await _taskRepository.GetTaskByIdAsync(id);
            if (task == null) return NotFound();
            return Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskDto dto)
        {
            var creatorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Validate assigned user exists
            if (!await _taskRepository.UserExistsAsync(dto.AssignedUserId))
                return BadRequest("Assigned user does not exist.");

            var task = new TaskItem
            {
                Title = dto.Title,
                IsDone = dto.IsDone,
                UserId = dto.AssignedUserId,
                CreatedByUserId = creatorId
            };

            await _taskRepository.CreateTaskAsync(task);
            await _taskRepository.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var task = await _taskRepository.GetTaskByIdAsync(id);
            if (task == null) return NotFound();

            // Allow assigned user OR creator to edit
            if (task.UserId != userId && task.CreatedByUserId != userId)
                return Forbid();

            // Validate assigned user exists
            if (!await _taskRepository.UserExistsAsync(dto.AssignedUserId))
                return BadRequest("Assigned user does not exist.");

            task.Title = dto.Title;
            task.IsDone = dto.IsDone;
            task.UserId = dto.AssignedUserId;

            await _taskRepository.UpdateTaskAsync(task);
            await _taskRepository.SaveChangesAsync();

            return Ok(task);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var task = await _taskRepository.GetTaskByIdAsync(id);
            if (task == null) return NotFound();

            // Only the creator can delete
            if (task.CreatedByUserId != userId)
                return Forbid();

            await _taskRepository.DeleteTaskAsync(task);
            await _taskRepository.SaveChangesAsync();

            return NoContent();
        }

    }
}