using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using task_manager_api.DTOs;
using TaskManager.Models;

namespace task_manager_api.Interfaces
{
    public interface ITaskRepository
    {
        Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync();
        Task<TaskItem?> GetTaskByIdAsync(int id);
        Task<TaskItem> CreateTaskAsync(TaskItem task);
        Task UpdateTaskAsync(TaskItem task);
        Task DeleteTaskAsync(TaskItem task);
        Task<bool> UserExistsAsync(int userId);
        Task<bool> SaveChangesAsync();
    }
}