using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

ApiController apiController = new ApiController();


app.MapGet("/", () => "Hello World!");

app.MapPost("/createUser", async (HttpContext context) =>
{
    try
    {
        // Assuming you want to read a JSON payload and deserialize it into a User object
        using (var reader = new StreamReader(context.Request.Body))
        {
            var body = await reader.ReadToEndAsync();
            var user = JsonSerializer.Deserialize<UserDto>(body);

            if (user == null)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return "Invalid user data.";
            }

            await Task.Run(() =>
            {
                // Log the user data (ensure sensitive data is not logged)
                Console.WriteLine(user);

                // Add user to the list (ensure thread-safety)
                lock (apiController.list)
                {
                    apiController.list.Add(user);
                }

                return "User created.";
            });

            context.Response.StatusCode = StatusCodes.Status201Created;
            return "User created.";
        }
    }
    catch (Exception ex)
    {
        // Log the exception
        Console.WriteLine($"Error: {ex.Message}");

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        return "An error occurred while creating the user.";
    }
});

app.MapGet("/getAllUsers", async (HttpContext context) =>
{
    try
    {
        context.Response.StatusCode = StatusCodes.Status200OK;
        var users = await Task.Run(() =>
        {
            return apiController.list;
        });

        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(users);
    }
    catch(Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsync(ex.Message);
    }
});


app.MapGet("/json", async () =>
{
    using HttpClient client = new HttpClient();

    HttpResponseMessage response = await client.GetAsync("https://jsonplaceholder.typicode.com/todos/1");

    response.EnsureSuccessStatusCode();

    string responseBody = await response.Content.ReadAsStringAsync();

    return responseBody;
});

app.MapGet("/api/{id}", (string id) =>
{
    var user = apiController.FindUserById(id);
    return Results.Json(user);
});

app.Run();






public record class UserDto(string name, string id);

public class ApiController
{
    public List<UserDto> list = new List<UserDto>()
    {
        new UserDto("олег", "1"),
        new UserDto("танк", "2"),
        new UserDto("арт", "3")
    };

    public UserDto FindByIdInList(string x)
    {
        foreach (var item in list)
        {
            if (item.id == x)
                return item;
        }
        return new UserDto("не найден", "1000");
    }

    public void PrintAllUsers()
    {

    }

    public UserDto FindUserById(string id)
    {
        return FindByIdInList(id);
    }
}
