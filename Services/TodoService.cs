using TodoApp.Models;

using Dapper;
using MySqlConnector;

namespace TodoApp.Services;

public class TodoService
{
    public List<TodoItem> Todos { get; } = new List<TodoItem>(); // List of all todo items managed by the service.
    public async Task<List<TodoItem>> GetTodosAsync(
        TodoStatus? status,
        TodoSort sort)
    {
        string sql = """
            SELECT Id, Title, Description, Status, Deadline
            FROM Todos
            """;

        if (status is not null)
        {
            sql += " WHERE Status = @Status";
        }

        switch (sort)
        {
            case TodoSort.Title:
                sql += " ORDER BY Title, Id";
                break;

            case TodoSort.Status:
                sql += " ORDER BY Status, Id";
                break;

            default:
                sql += " ORDER BY Deadline IS NULL, Deadline, Id";
                break;
        }

        await using var connection =
            new MySqlConnection(connectionString);

        await connection.OpenAsync();

        var rows = await connection.QueryAsync<TodoRow>(
            sql,
            new { Status = (int?)status });

        return rows.Select(row => new TodoItem
        {
            Id = row.Id,
            Title = row.Title,
            Description = row.Description,
            Status = (TodoStatus)row.Status,
            Deadline = row.Deadline.HasValue
                ? DateOnly.FromDateTime(row.Deadline.Value)
                : null
        }).ToList();
    }
    public async Task<TodoItem?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT Id, Title, Description, Status, Deadline
            FROM Todos
            WHERE Id = @Id
            """;

        await using var connection =
            new MySqlConnection(connectionString);

        await connection.OpenAsync();

        TodoRow? row = await connection.QuerySingleOrDefaultAsync<TodoRow>(
            sql,
            new { Id = id });

        if (row is null)
        {
            return null;
        }

        return new TodoItem
        {
            Id = row.Id,
            Title = row.Title,
            Description = row.Description,
            Status = (TodoStatus)row.Status,
            Deadline = row.Deadline.HasValue
                ? DateOnly.FromDateTime(row.Deadline.Value)
                : null
        };
    }
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
    public IEnumerable<TodoItem> GetTodos(
        TodoStatus? status,
        TodoSort sort)
    {
        IEnumerable<TodoItem> result = Todos;

        if (status is not null)
        {
            result = result.Where(todo => todo.Status == status.Value);
        }

        switch (sort)
        {
            case TodoSort.Title:
                result = result.OrderBy(todo => todo.Title);
                break;

            case TodoSort.Status:
                result = result.OrderBy(todo => todo.Status);
                break;

            case TodoSort.Deadline:
                result = result
                    .OrderBy(todo => todo.Deadline is null)
                    .ThenBy(todo => todo.Deadline);
                break;
        }

        return result;
    }
    
    private readonly string connectionString;

    public TodoService(IConfiguration configuration)
    {
        connectionString =
            configuration.GetConnectionString("TodoDatabase")
            ?? throw new InvalidOperationException(
                "Die Datenbankverbindung TodoDatabase fehlt.");
    }
    
    public async Task<int> CountAsync()
    {
        await using var connection =
            new MySqlConnection(connectionString);

        await connection.OpenAsync();

        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Todos;");
    }

    private sealed class TodoRow
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public int Status { get; set; }
        public DateTime? Deadline { get; set; }
    }
}
