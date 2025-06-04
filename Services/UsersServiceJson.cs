using Microsoft.AspNetCore.Mvc;
using webApiProject.Interfaces;
using webApiProject.Models;

namespace webApiProject.Services;

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;

public class UsersServiceJson : IService<Users>
{
    private readonly ActiveUserService activeUserService;
    private readonly Func<IService<Tasks>> tasksServiceFactory; // Factory ליצירת השירות Tasks
    private List<Users> usersList;
    private static string fileName = "Users.json";
    private string filePath;

    public UsersServiceJson(
        IHostEnvironment env,
        ActiveUserService activeUserService,
        Func<IService<Tasks>> tasksServiceFactory
    )
    {
        try
        {
            this.tasksServiceFactory = tasksServiceFactory;
            this.activeUserService = activeUserService;
            filePath = Path.Combine(env.ContentRootPath, "Data", fileName);

            if (!File.Exists(filePath))
            {
                usersList = new List<Users>();
                saveToFile();
                return;
            }

            using (var jsonFile = File.OpenText(filePath))
            {
                usersList =
                    JsonSerializer.Deserialize<List<Users>>(
                        jsonFile.ReadToEnd(),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    ) ?? new List<Users>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing UsersServiceJson: {ex.Message}");
            usersList = new List<Users>();
        }
    }

    private void saveToFile()
    {
        try
        {
            File.WriteAllText(filePath, JsonSerializer.Serialize(usersList));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving to file: {ex.Message}");
            throw;
        }
    }

    public List<Users> Get()
    {
        try
        {
            return usersList ?? new List<Users>();
        }
        catch (Exception ex)
        {
            return new List<Users>();
        }
    }

    public List<Users> GetAllItems() => null;

    public Users GetMyUser()
    {
        try
        {
            return usersList.FirstOrDefault(u => u.Id == activeUserService.UserId) ?? new Users();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetMyUser: {ex.Message}");
            return null;
        }
    }

    public Users Get(int id)
    {
        try
        {
            return usersList.FirstOrDefault(u => u.Id == id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Get by ID: {ex.Message}");
            return null;
        }
    }

    public int Insert(Users newItem)
    {
        try
        {
            Console.WriteLine("in Insert");
            if (!CheckValueRequest(newItem))
                return -1;

            int lastId = usersList.Any() ? usersList.Max(s => s.Id) : 0;
            newItem.Id = lastId + 1;
            usersList.Add(newItem);
            saveToFile();
            return newItem.Id;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Insert: {ex.Message}");
            return -1;
        }
    }

    public bool Update(int id, Users newItem)
    {
        try
        {
            if (!CheckValueRequest(newItem) || newItem.Id != id)
                return false;
            var user = usersList.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return false;

            user.UserName = newItem.UserName;
            user.Password = newItem.Password;
            user.Email = newItem.Email;
            user.Type = newItem.Type;
            saveToFile();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Update: {ex.Message}");
            return false;
        }
    }

    public bool Delete(int id)
    {
        try
        {
            //אי אפשר למחוק את עצמו
            if (activeUserService.UserId == id)
            {
                return false;
            }

            var tasksService = tasksServiceFactory();

            // מחיקת כל הנעליים של המשתמש
            List<Tasks> userTasks = tasksService.Get().Where(task => task.UserId == id).ToList();
            foreach (var task in userTasks)
            {
                if (!tasksService.Delete(task.Id))
                {
                    return false;
                }
            }

            // מחיקת המשתמש
            var item = usersList.FirstOrDefault(s => s.Id == id);
            if (item == null)
                return false;

            usersList.Remove(item);
            saveToFile();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Delete: {ex.Message}");
            return false;
        }
    }

    private bool CheckValueRequest(Users newItem)
    {
        try
        {
            if (
                newItem == null
                || string.IsNullOrWhiteSpace(newItem.Password)
                || string.IsNullOrWhiteSpace(newItem.UserName)
            )
                return false;

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CheckValueRequest: {ex.Message}");
            return false;
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            if (string.IsNullOrEmpty(email))
                return false;

            // ביטוי רגולרי לבדוק אם המייל תקין
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in IsValidEmail: {ex.Message}");
            return false;
        }
    }
}

public static partial class ServiceUlilities
{
    public static IServiceCollection AddUserService(this IServiceCollection services)
    {
        services.AddScoped<UsersServiceJson>();
        return services;
    }
}
