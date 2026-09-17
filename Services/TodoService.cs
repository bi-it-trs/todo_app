using TodoApp.Models;
namespace TodoApp.Services;

public class TodoService
{
    public List<TodoItem> Todos { get; } = new List<TodoItem>(); // List of all todo items managed by the service.

    private int nextId = 1; // Tracks the next available ID for new todo items.

    public void Add // Adds a new todo item to the list.
    (
        string title,
        string description,
        TodoStatus status,
        DateOnly? deadline
    )
    
    {
        TodoItem todo = new TodoItem // Create a new todo item with the provided details.
        {
            Id = nextId,
            Title = title,
            Description = description,
            Status = status,
            Deadline = deadline
        };

        nextId++;

        Todos.Add(todo); // Add the newly created todo item to the list.
    }

    public TodoItem? GetById(int id) // Retrieves a todo item by its ID, or null if not found.
    {
        return Todos.FirstOrDefault(todo => todo.Id == id); // Retrieves a todo item by its ID, or null if not found.
    }

    public void Update(TodoItem todo) // Updates an existing todo item in the list.
    {
        var existingTodo = GetById(todo.Id);
        if (existingTodo is not null)
        {
            existingTodo.Title = todo.Title;
            existingTodo.Description = todo.Description;
            existingTodo.Status = todo.Status;
            existingTodo.Deadline = todo.Deadline;
        }
    }
    public void Delete(int id)
    {
        TodoItem? todo = GetById(id);
        if (todo is not null)
        {
            Todos.Remove(todo);
        }
    }
}