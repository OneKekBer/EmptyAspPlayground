using System.Collections.Generic;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using System;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

//ApiController apiController = new ApiController();
DB db = new DB();

app.MapGet("/", () => "Hello World!");


app.MapPost("/login", async (HttpContext ctx) => 
{
    using var reader = new StreamReader(ctx.Request.Body);

    var body = await reader.ReadToEndAsync();

    try
    {
        var registerData = JsonSerializer.Deserialize<RegisterDto>(body);

        var existingUser = db.LogIn(registerData);

        ctx.Response.StatusCode = StatusCodes.Status200OK;
        await ctx.Response.WriteAsync($"You loged in successfully your id: {existingUser.Id}");
        return;
    }
    catch (JsonException)
    {
        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
        await ctx.Response.WriteAsync("Login or password are in incorrect type");
        return;
    }
    catch (Exception ex)
    {
        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
        await ctx.Response.WriteAsync(ex.Message);
        return;
    }
});

app.MapPost("/register", async (HttpContext ctx) =>
{
    using var reader = new StreamReader(ctx.Request.Body);

    var body = await reader.ReadToEndAsync();
   
    try
    {
        var registerData = JsonSerializer.Deserialize<RegisterDto>(body);

        db.RegisterUser(registerData);

        ctx.Response.StatusCode = StatusCodes.Status200OK;
        await ctx.Response.WriteAsync("User was successfully created");
        return;
    }
    catch (JsonException)
    {
        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
        await ctx.Response.WriteAsync("Login or password are in incorrect type");
        return;
    }
    catch (Exception ex)
    {
        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
        await ctx.Response.WriteAsync(ex.Message);
        return;
    }
});




//app.MapPost("/createUser", async (HttpContext context) =>
//{
//    try
//    {
//        // Assuming you want to read a JSON payload and deserialize it into a User object
//        using (var reader = new StreamReader(context.Request.Body))
//        {
//            var body = await reader.ReadToEndAsync();
//            var user = JsonSerializer.Deserialize<UserDto>(body);

//            if (user == null)
//            {
//                context.Response.StatusCode = StatusCodes.Status400BadRequest;
//                await context.Response.WriteAsync("User is not defined.");
//                return;
//            }

//            if (!apiController.IsIdUnique(user.id))
//            {
//                context.Response.StatusCode = StatusCodes.Status409Conflict;
//                await context.Response.WriteAsync("User ID already exists.");
//                return;
//            }

//            await Task.Run(() =>
//            {
//                // Log the user data (ensure sensitive data is not logged)
//                Console.WriteLine(user);

//                // Add user to the list (ensure thread-safety)
//                lock (apiController.list)
//                {
                    
//                    apiController.list.Add(user);
//                }
//            });

//            context.Response.StatusCode = StatusCodes.Status201Created;
//            await context.Response.WriteAsync("User created.");
//            return;
//        }
//    }
//    catch (Exception ex)
//    {
//        // Log the exception
//        Console.WriteLine($"Error: {ex.Message}");

//        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
//        await context.Response.WriteAsync("Error with creating user.");
//        return;
//    }
//});

//app.MapGet("/getAllUsers", async (HttpContext context) =>
//{
//    try
//    {
//        context.Response.StatusCode = StatusCodes.Status200OK;
//        var users = await Task.Run(() =>
//        {
//            return db.users;
//        });

//        context.Response.ContentType = "application/json";

//        await context.Response.WriteAsJsonAsync(users);
//    }
//    catch(Exception ex)
//    {
//        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

//        await context.Response.WriteAsync(ex.Message);
//    }
//});


//app.MapGet("/json", async () =>
//{
//    using HttpClient client = new HttpClient();

//    HttpResponseMessage response = await client.GetAsync("https://jsonplaceholder.typicode.com/todos/1");

//    response.EnsureSuccessStatusCode();

//    string responseBody = await response.Content.ReadAsStringAsync();

//    return responseBody;
//});

//app.Run();





record class LoginDto(string name, string password);
public class UserData
{
    public Guid Id { get; } = Guid.NewGuid();

    public string Login { get; init; }

    public string PasswordHash { get; init; }

    public UserData(string login, string password)
    {
        Login = login;
        PasswordHash = password;
    }
}

public class RegisterDto
{
    public string Login { get; init; }
    public string Password { get; init; }
};

public class DB
{
    public List<UserData> users = new List<UserData>();

    public bool IsLoginExists(string login)
    {
        foreach (var item in users)
        {
            if (item.Login == login) // бля сравнивать строки == просто гениально
            {
                return true;
            }
        }
        return false;
    }

    public UserData FindUserByLogin(string login)
    {
        foreach (var item in users)
        {
            if (item.Login == login) // бля сравнивать строки == просто гениально
            {
                return item;
            }
        }
        throw new Exception($"User with this login:{login} not found");
    }

    private static string ConvertPasswordToHash(string password)
    {
        return Encoding.UTF8.GetString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
    }

    private static bool IsPasswordHashesEquals(UserData user, string incomePassword)
    {
        if(user.PasswordHash == ConvertPasswordToHash(incomePassword))
            return true;
        return false;
    }

    

    public UserData LogIn(RegisterDto registerData) 
    {
        var user = FindUserByLogin(registerData.Login);

        if (!IsPasswordHashesEquals(user, registerData.Password))
            throw new Exception("Password is incorrect");
        
        return user;
    }

    public UserData RegisterUser(RegisterDto registerData)
    {
        if (IsLoginExists(registerData.Login))
            throw new Exception("Login already used");

        var createdUser = new UserData(registerData.Login, ConvertPasswordToHash(registerData.Password));
        users.Add(createdUser);
        return createdUser;
    }

}

////

//public record class UserDto(string name, int id);

//public class ApiController
//{
//    public List<UserDto> list = new List<UserDto>()
//    {
//        new UserDto("олег", 1),
//        new UserDto("танк", 2),
//        new UserDto("арт", 3)
//    };

//    public UserDto FindUserById(int x)
//    {
//        foreach (var item in list)
//        {
//            if (item.id == x)
//                return item;
//        }
//        return new UserDto("не найден", 1000);
//    }

//    public void PrintAllUsers()
//    {

//    }

//    public bool IsIdUnique(int id)
//    {
//        //return !list.Any(x => x.id == id);
//        foreach (var item in list)
//        {
//            if(item.id == id)
//            {
//                return false;
//            }
//        }
//        return true;
//    }


//}
