using Microsoft.EntityFrameworkCore;
using task_manager_api.DTOs;
using task_manager_api.Interfaces;
using TaskManager.Data;
using TaskManager.Models;

namespace task_manager_api.Data
{
    public class TaskRepository(ApplicationDbContext context) : ITaskRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync()
        {
            return await _context.Tasks
                .Include(t => t.User)
                .Include(t => t.CreatedByUser)
                .Select(t => new TaskResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    IsDone = t.IsDone,
                    AssignedUserId = t.UserId,
                    AssignedUserEmail = t.User.Email,
                    CreatedByUserId = t.CreatedByUserId,
                    CreatedByUserEmail = t.CreatedByUser.Email
                })
                .ToListAsync();
        }

        public async Task<TaskItem?> GetTaskByIdAsync(int id)
        {
            return await _context.Tasks
                .Include(t => t.User)
                .Include(t => t.CreatedByUser)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<TaskItem> CreateTaskAsync(TaskItem task)
        {
            _context.Tasks.Add(task);
            return task;
        }

        public async Task UpdateTaskAsync(TaskItem task)
        {
            _context.Tasks.Update(task);
        }

        public async Task DeleteTaskAsync(TaskItem task)
        {
            _context.Tasks.Remove(task);
        }

        public async Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}