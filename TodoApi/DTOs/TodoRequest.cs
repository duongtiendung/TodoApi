using static TodoApi.Models.Enums;
using TodoApi.Models;

namespace TodoApi.DTOs
{
    public class TodoRequest
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public Status Status { get; set; }
    }
}
