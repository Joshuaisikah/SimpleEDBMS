// src/SimpleRDBMS.Application/DTOs/ExecutionResultDto.cs
namespace SimpleRDBMS.Application.DTOs
{
    public class ExecutionResultDto
    {
        public bool Success { get; set; }
        public int RowsAffected { get; set; }
        public string Message { get; set; }
    }
}