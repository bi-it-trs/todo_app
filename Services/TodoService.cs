using TodoApp.Models;
namespace TodoApp.Services;

public class TodoService
{
    public List<TodoItem> Todos { get; } = new List<TodoItem>();

    private int nextId = 1;

    public void Add
    (
        string title,
        string description,
        TodoStatus status,
        DateOnly? deadline
    )
    
    {
        TodoItem todo = new TodoItem
        {
            Id = nextId,
            Title = title,
            Description = description,
            Status = status,
            Deadline = deadline
        };

        nextId++;

        Todos.Add(todo);
    }

}